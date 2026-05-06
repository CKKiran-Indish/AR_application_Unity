from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Dummy database (UPDATED FORMAT)
data_store = {
    "123": {
        "machineName": "Pump A",
        "status": "Running",
        "parameters": {
            "temperature": "45",
            "pressure": "12",
            "speed": "1500 RPM",
            "test1": "value1",
            "test2": "value2",
            "test3": "value3",
            "test4": "value4"
        }
    },
    "456": {
        "machineName": "Motor B",
        "status": "Stopped",
        "parameters": {
            "temperature": "30",
            "voltage": "220V",
            "current": "5A",
            "test1": "value1",
            "test2": "value2",
            "test3": "value3"
        }
    }
}

@app.route('/data', methods=['GET'])
def get_data():
    qr_id = request.args.get('qr')   #  match Unity request (?qr=...)

    if qr_id in data_store:
        return jsonify(data_store[qr_id])  #  DIRECT RETURN (important)
    else:
        return jsonify({
            "machineName": "Unknown",
            "status": "Error",
            "parameters": {
                "message": "Data not found"
            }
        }), 404
@app.route('/health')
def health():
    return jsonify({"status": True})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)