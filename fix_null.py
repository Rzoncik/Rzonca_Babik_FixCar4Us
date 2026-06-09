import sqlite3

def run():
    conn = sqlite3.connect('database.db')
    cursor = conn.cursor()
    cursor.execute('UPDATE RepairOrders SET EmployeeId = 1 WHERE EmployeeId IS NULL')
    conn.commit()
    conn.close()
    print("Fixed!")

if __name__ == '__main__':
    run()
