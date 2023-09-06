import flask 
import os 
import sqlite3 
import docker 
import sys
import flask_sock
import json
import flask_cors
import platform
import psutil
import cpuinfo
import subprocess
from logger import Logger, LogType
from config import app_port
logger = Logger()

def convert_bytes(bytes_value, to_unit):
    if to_unit == "GB":
        return int(bytes_value / (1024 ** 3)) 
    elif to_unit == "MB":
        return int(bytes_value / (1024 ** 2))  
    else:
        return bytes_value  


def check_and_fail_on_windows():
    if platform.system() == "Windows":
        logger.log(LogType.Error , "This script cannot run on Windows.")
        sys.exit(1)

def sqlquery(sql, *parameter):
    conn = sqlite3.connect("database.sqlite", check_same_thread=False)
    cursor = conn.cursor()
    data = cursor.execute(sql, (parameter)).fetchall()
    conn.commit()
    return data

def start_service(service_name):
    subprocess.run(['sudo', 'systemctl', 'start', service_name])

def stop_service(service_name):
    subprocess.run(['sudo', 'systemctl', 'stop', service_name])

def restart_service(service_name):
    subprocess.run(['sudo', 'systemctl', 'restart', service_name])

def check_service_status(service_name):
    result = subprocess.run(['systemctl', 'is-active', service_name], capture_output=True, text=True)
    status = result.stdout.strip()
    return status

app_dir = "/etc/KosmaPanel"

logger.log(LogType.Info, "Welcome to KosmaPanel daemon please wait while we start our services")
logger.log(LogType.Info, f"Mounting daemon dir to {app_dir}")
try: 
    os.chdir(app_dir)
except Exception as ex: 
    logger.log(LogType.Error, f"Failed to mount to '{app_dir}': {ex}")

logger.log(LogType.Info, "Starting docker..")
try: 
    client = docker.from_env()
    start_service('docker')
except Exception as ex:
    logger.log(LogType.Error, "Failed to start docker: "+ex)
logger.log(LogType.Info, "Started docker")

logger.log(LogType.Info, "Starting webserver..")
try:
    app = flask.Flask(__name__)
except Exception as ex:
    logger.log(LogType.Error, "Failed to start webserver: "+ex)
logger.log(LogType.Info, f"Started webserver at: {app_port}")
logger.log(LogType.Info, "Starting websockets..")
try:
    sock = flask_sock.Sock(app)
except Exception as ex:
    logger.log(LogType.Error,"Failed to start websockets: "+ex)
logger.log(LogType.Info, "Started websockets")
logger.log(LogType.Info, "Starting cors..") 
try: 
    cors = flask_cors.CORS(app, resources={r"/*": {"origins": "*"}})
except Exception as ex:
    logger.log(LogType.Error, "Failed to start cors: "+ex)
logger.log(LogType.Info,"Started cors")
logger.log(LogType.Info,"Running system scan..")
try:
    check_and_fail_on_windows()
except Exception as ex:
    logger.log(LogType.error, "Failed to finish system scan: "+ex)

@app.errorhandler(400)
def bad_request_error(error):
    response = flask.jsonify({"error": "Bad Request"})
    response.status_code = 400
    return response

@app.errorhandler(401)
def unauthorized_error(error):
    response = flask.jsonify({"error": "Unauthorized"})
    response.status_code = 401
    return response

@app.errorhandler(403)
def forbidden_error(error):
    response = flask.jsonify({"error": "Forbidden"})
    response.status_code = 403
    return response

@app.errorhandler(404)
def not_found_error(error):
    response = flask.jsonify({"error": "Not Found"})
    response.status_code = 404
    return response

@app.errorhandler(405)
def not_found_error(error):
    response = flask.jsonify({"error": "Method Not Allowed"})
    response.status_code = 405
    return response

@app.errorhandler(500)
def internal_server_error(error):
    response = flask.jsonify({"error": "Internal Server Error"})
    response.status_code = 500
    return response

@app.errorhandler(503)
def internal_server_error(error):
    response = flask.jsonify({"error": "Internal Server Error"})
    response.status_code = 503
    return response

@app.route("/")
def main():
    response = flask.jsonify({"success": "node online"})
    response.headers.add("Access-Control-Allow-Origin", "*")
    return response

@app.route("/api/daemon/restart", methods=["POST"])
def route_restart_daemon():
    system_token = flask.request.form.get("system_token")
    if not all([system_token]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            restart_service("daemon")
            response = {
                "success": "Service restarted successfully",
            }
            return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)

@app.route("/api/system/reboot", methods=["POST"])
def route_shutdown_system():
    system_token = flask.request.form.get("system_token")
    if not all([system_token]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            reboot_command = "reboot" 
            try:
                subprocess.run(reboot_command, shell=True, check=True)
            except subprocess.CalledProcessError as e:
                response = {
                    "error": Exception(f"Error: {e}"),
                }
                return json.dumps(response), 500, {'Content-Type': 'application/json'}
            else:
                response = {
                    "success": "Server is rebooting...",
                }
                return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)

@app.route("/api/system/shutdown", methods=["POST"])
def route_reboot_system():
    system_token = flask.request.form.get("system_token")
    if not all([system_token]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            poweroff_command = "poweroff" 
            try:
                subprocess.run(poweroff_command, shell=True, check=True)
            except subprocess.CalledProcessError as e:
                response = {
                    "error": Exception(f"Error: {e}"),
                }
                return json.dumps(response), 500, {'Content-Type': 'application/json'}
            else:
                response = {
                    "success": "Server is shutting down...",
                }
                return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)

@app.route("/api/daemon/shutdown", methods=["POST"])
def route_shutdown_daemon():
    system_token = flask.request.form.get("system_token")
    if not all([system_token]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            stop_service("daemon")
            response = {
                "success": "Shutdown successfully",
            }
            return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)

@app.route("/api/daemon/info",methods=["POST"])
def route_get_daemon_info():
    system_token = flask.request.form.get("system_token")
    if not all([system_token]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            # Get the OS name
            os_name = platform.system()
            
            # Get disk info
            disk_info = psutil.disk_usage('/')

            # Get RAM info
            ram_info = psutil.virtual_memory()

            # Get CPU info
            cpu_info = cpuinfo.get_cpu_info()
            cpu_name = cpu_info['brand_raw']
            # Get kernel name
            kernel_name = platform.uname().release
            # Get ram
            ram_total_mb = convert_bytes(ram_info.total, "MB")
            ram_available_mb = convert_bytes(ram_info.available, "MB")
            ram_used_mb = convert_bytes(ram_info.used, "MB")
            ram_free_mb = convert_bytes(ram_info.free, "MB")
            # Get disk
            disk_total_gb = convert_bytes(disk_info.total, "GB")
            disk_used_gb = convert_bytes(disk_info.used, "GB")
            disk_free_gb = convert_bytes(disk_info.free, "GB")
            uptime_seconds = int(psutil.boot_time())

            #  Convert uptime to a human-readable format (days, hours, minutes)
            uptime_minutes, uptime_seconds = divmod(uptime_seconds, 60)
            uptime_hours, uptime_minutes = divmod(uptime_minutes, 60)
            uptime_days, uptime_hours = divmod(uptime_hours, 24)
            # Prepare the response as a JSON object
            response = {
                "os_type": os_name,
                "kernel": kernel_name,
                "uptime": {
                    "days": uptime_days,
                    "hours": uptime_hours,
                    "minutes": uptime_minutes,
                },
                "disk_info": {
                    "total": f"{disk_total_gb} GB",
                    "used": f"{disk_used_gb} GB",
                    "free": f"{disk_free_gb} GB",
                },
                "ram_info": {
                    "total": f"{ram_total_mb} MB",
                    "available": f"{ram_available_mb} MB",
                    "used": f"{ram_used_mb} MB",
                    "free": f"{ram_free_mb} MB",
                },
                "cpu_info": {
                    "name": cpu_name,
                }
            }

            return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)
    
@app.route("/api/servers/<uuid>/create", methods=["POST"])
def route_create_server(uuid):
    system_token = flask.request.form.get("system_token")
    user_token = flask.request.form.get("user_token")
    port = flask.request.form.get("port")
    memory = flask.request.form.get("memory")
    
    if not all([system_token, user_token, port, memory]):
        response = {
            "error": "Missing required input parameters",
        }
        return json.dumps(response), 400, {'Content-Type': 'application/json'}

    if system_token == app.config["SYSTEM_TOKEN"]:
        try:
            os.mkdir("/etc/KosmaPanel/data/{}".format(uuid))
            sqlquery("INSERT INTO containers (uuid, user_token, port, memory) VALUES (?, ?, ?, ?)", uuid, user_token, port, memory)
            response = {
                "message": "Server created",
            }
            return json.dumps(response), 200, {'Content-Type': 'application/json'}
        except Exception as e:
            response = {
                "error": str(e),
            }
            return json.dumps(response), 500, {'Content-Type': 'application/json'}
    else:
        flask.abort(401)

try:
    if sys.argv[1] == "--token":
        conn = sqlite3.connect("database.sqlite", check_same_thread=False)
        cursor = conn.cursor()
        cursor.executescript(open("schema.sql").read())
        conn.commit()
        if not os.path.exists("/etc/KosmaPanel/data"):
            os.mkdir("/etc/KosmaPanel/data")
        logger.log(LogType.Info ,"-> Node configured succesfully")
        logger.log(LogType.Info, "Run: service deamon start, to start deamon via systemctl")
        cursor.execute("INSERT INTO settings (system_token) VALUES (?)", (sys.argv[2],))
        conn.commit()
        os._exit(1)
except:
    if os.path.isfile("database.sqlite"):
        app.config["SYSTEM_TOKEN"] = sqlquery("SELECT * FROM settings")[0][0]
        app.config["SECRET_KEY"] = os.urandom(30).hex()
        app.config["UPLOAD_FOLDER"] = "/etc/KosmaPanel/data"
        app.run(debug=False, host="0.0.0.0", port=app_port)
    else:
        logger.log(LogType.Info,"Node not configured")
        os._exit(1)