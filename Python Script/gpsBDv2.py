import serial
import pyodbc

def connect_to_database():
    # Replace with your actual database connection details
    server = 'techallenge.database.windows.net'
    database = 'MonitFlota'
    username = 'tech_admin'
    password = 'Parola1234'
    driver = '{ODBC Driver 18 for SQL Server}'  # Make sure you have this driver installed

    connection_string = f"DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password}"
    conn = pyodbc.connect(connection_string)
    return conn

def save_coordinates(conn, vehicle_id, coordinates, speed, potholes):
    cursor = conn.cursor()
    query = """INSERT INTO currentloc (vehicle_id, coordinates, speed, potholes) 
               VALUES (?, ?, ?, ?)"""
    cursor.execute(query, (vehicle_id, coordinates, speed, potholes))
    conn.commit()
    cursor.close()

def main():
    print("Starting GPS Coordinates Reader...")

    # Set up serial port
    port = "/dev/ttyUSB0"  # Change this to your actual port
    baudrate = 115200

    try:
        conn = connect_to_database()
        print("Connected to the database.")

        with serial.Serial(port, baudrate, timeout=1) as ser:
            print("Listening for GPS data...")

            while True:
                # Read a line from the serial port as bytes
                data_line = ser.readline()  # Read bytes

                try:
                    # Decode bytes to string, ignore errors
                    decoded_line = data_line.decode('utf-8', errors='ignore').strip()

                    # Look for the "Longitude, Latitude" prefix to parse coordinates
                    if "Longitude, Latitude" in decoded_line:
                        # Read the next two lines for latitude and longitude
                        latitude_line = ser.readline().decode('utf-8', errors='ignore').strip()
                        longitude_line = ser.readline().decode('utf-8', errors='ignore').strip()

                        try:
                            latitude = float(latitude_line)
                            longitude = float(longitude_line)

                            coordinates = f"{latitude},{longitude}"
                            

                            # Skip the "Speed (km/h):" label and read the actual speed value
                            ser.readline()  # Skip the label "Speed (km/h):"
                            speed_line = ser.readline().decode('utf-8', errors='ignore').strip()  # Read the next line (speed value)
                            speed = float(speed_line) if speed_line else 0.0

                            # Skip the "Potholes:" label and read the actual potholes value
                            ser.readline()  # Skip the label "Potholes:"
                            potholes_line = ser.readline().decode('utf-8', errors='ignore').strip()  # Read the next line (potholes value)
                            potholes = int(potholes_line) if potholes_line else 0

                            coordinatesPrint = f"{coordinates}, Speed: {speed}, Potholes: {potholes}"
                            print(f"Coordinates: {coordinatesPrint}")

                            # Save to database
                            save_coordinates(conn, vehicle_id=1, coordinates=coordinates, speed=speed, potholes=potholes)

                        except ValueError as e:
                            print(f"Invalid data format for latitude, longitude, speed, or potholes: {e}")
                except UnicodeDecodeError as e:
                    print(f"Decoding error: {e}")

    except Exception as e:
        print(f"Error: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    main()
