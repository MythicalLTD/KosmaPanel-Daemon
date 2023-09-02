rm KosmaPanel
pyinstaller --onefile --name KosmaPanel --clean app.py
cp dist/KosmaPanel /etc/KosmaPanel/