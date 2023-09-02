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
import datetime

class LogType:
    Info = "Info"
    Warning = "Warning"
    Error = "Error"

class Logger:
    def __init__(self):
        log_directory = "logs"
        log_file_name = "log.txt"
        log_directory_path = os.path.join(os.getcwd(), log_directory)
        self.log_file_path = os.path.join(log_directory_path, log_file_name)
        os.makedirs(log_directory_path, exist_ok=True)
        self._rename_log_file()

    def log(self, log_type, message):
        timestamp = datetime.datetime.now().strftime("%H:%M:%S")
        log_text = f"[{timestamp}] [{log_type}] {message}"

        color = '\033[0m' 
        if log_type == LogType.Info:
            color = '\033[92m' 
        elif log_type == LogType.Warning:
            color = '\033[93m'  
        elif log_type == LogType.Error:
            color = '\033[91m' 

        print(color + log_text + '\033[0m')  

        self._append_to_file(log_text)

    def _append_to_file(self, log_text):
        try:
            with open(self.log_file_path, "a") as file:
                file.write(log_text + "\n")
        except Exception as ex:
            print(f"Error writing to log file: {ex}")

    def _rename_log_file(self):
        if os.path.exists(self.log_file_path):
            log_file_name_without_extension = os.path.splitext(os.path.basename(self.log_file_path))[0]
            log_file_extension = os.path.splitext(self.log_file_path)[1]
            log_directory_path = os.path.dirname(self.log_file_path)
            new_log_file_name = self._get_unique_log_file_name(log_directory_path, log_file_name_without_extension, log_file_extension)
            new_log_file_path = os.path.join(log_directory_path, new_log_file_name)
            os.rename(self.log_file_path, new_log_file_path)
            self.log_file_path = new_log_file_path

    def _get_unique_log_file_name(self, directory_path, file_name_without_extension, file_extension):
        unique_file_name = f"{file_name_without_extension}-{datetime.datetime.now().strftime('%Y-%m-%d')}{file_extension}"
        unique_file_path = os.path.join(directory_path, unique_file_name)

        counter = 1
        while os.path.exists(unique_file_path):
            unique_file_name = f"{file_name_without_extension}-{counter}-{datetime.datetime.now().strftime('%Y-%m-%d')}{file_extension}"
            unique_file_path = os.path.join(directory_path, unique_file_name)
            counter += 1

        return unique_file_name

def convert_bytes(bytes_value, to_unit):
    if to_unit == "GB":
        return int(bytes_value / (1024 ** 3)) 
    elif to_unit == "MB":
        return int(bytes_value / (1024 ** 2))  
    else:
        return bytes_value  


def check_and_fail_on_windows():
    if platform.system() == "Windows":
        Logger.log(LogType.Error , "This script cannot run on Windows.")
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

os.chdir("/etc/KosmaPanel")

client = docker.from_env()
app = flask.Flask(__name__)
sock = flask_sock.Sock(app)
cors = flask_cors.CORS(app, resources={r"/*": {"origins": "*"}})
check_and_fail_on_windows()

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
def restart_daemon():
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

@app.route("/api/daemon/shutdown", methods=["POST"])
def shutdown_daemon():
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
def get_daemon_info():
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
def create_server(uuid):
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
        Logger().log(LogType.Info ,"-> Node configured succesfully")
        Logger().log(LogType.Info, "Run: service deamon start, to start deamon via systemctl")
        cursor.execute("INSERT INTO settings (system_token) VALUES (?)", (sys.argv[2],))
        conn.commit()
        os._exit(1)
except:
    if os.path.isfile("database.sqlite"):
        app.config["SYSTEM_TOKEN"] = sqlquery("SELECT * FROM settings")[0][0]
        app.config["SECRET_KEY"] = os.urandom(30).hex()
        app.config["UPLOAD_FOLDER"] = "/etc/KosmaPanel/data"
        app.run(debug=False, host="0.0.0.0", port=5001)
    else:
        Logger().log(LogType.Info,"Node not configured")
        os._exit(1)