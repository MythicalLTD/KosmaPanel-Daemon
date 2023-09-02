import flask, os, sqlite3, docker, sys, flask_sock, json, flask_cors, time
from logger import Logger, LogType
logger = Logger()

def sqlquery(sql, *parameter):
    conn = sqlite3.connect("database.sqlite", check_same_thread=False)
    cursor = conn.cursor()
    data = cursor.execute(sql, (parameter)).fetchall()
    conn.commit()
    return data

os.chdir("/etc/KosmaPanel")

client = docker.from_env()
app = flask.Flask(__name__)
sock = flask_sock.Sock(app)
cors = flask_cors.CORS(app, resources={r"/*": {"origins": "*"}})

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

@app.route("/")
def main():
    response = flask.jsonify({"succes": "node online"})
    response.headers.add("Access-Control-Allow-Origin", "*")
    return response


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
        app.run(debug=False, host="0.0.0.0", port=5001)
    else:
        logger.log(LogType.Info,"Node not configured")
        os._exit(1)