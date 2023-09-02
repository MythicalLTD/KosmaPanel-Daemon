import os
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
