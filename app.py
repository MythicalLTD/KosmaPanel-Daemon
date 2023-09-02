import flask, os, sqlite3, docker, sys, flask_sock, json, flask_cors, time

def sqlquery(sql, *parameter):
    conn = sqlite3.connect("database.db", check_same_thread=False)
    cursor = conn.cursor()
    data = cursor.execute(sql, (parameter)).fetchall()
    conn.commit()
    return data

os.chdir("/etc/KosmaPanel")

client = docker.from_env()
app = flask.Flask(__name__)
sock = flask_sock.Sock(app)
cors = flask_cors.CORS(app, resources={r"/*": {"origins": "*"}})

@app.route("/")
def main():
    response = flask.jsonify({"succes": "node online"})
    response.headers.add("Access-Control-Allow-Origin", "*")
    return response

try:
    if sys.argv[1] == "--token":
        conn = sqlite3.connect("database.db", check_same_thread=False)
        cursor = conn.cursor()
        cursor.executescript(open("schema.sql").read())
        conn.commit()
        if not os.path.exists("/etc/KosmaPanel/data"):
            os.mkdir("/etc/KosmaPanel/data")
        print("\n-> Node configured succesfully")
        print("-> Enter: service deamon start, to start deamon\n")
        cursor.execute("INSERT INTO settings (system_token) VALUES (?)", (sys.argv[2],))
        conn.commit()
        os._exit(1)
except:
    if os.path.isfile("database.db"):
        app.config["SYSTEM_TOKEN"] = sqlquery("SELECT * FROM settings")[0][0]
        app.config["SECRET_KEY"] = os.urandom(30).hex()
        app.config["UPLOAD_FOLDER"] = "/etc/KosmaPanel/data"
        app.run(debug=False, host="0.0.0.0", port=5001)
    else:
        print("\n-> Node not configured")
        os._exit(1)