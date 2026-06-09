import sqlite3
import random

def run():
    conn = sqlite3.connect('database.db')
    cursor = conn.cursor()
    
    try:
        cursor.execute('ALTER TABLE RepairOrders ADD COLUMN EmployeeId INTEGER REFERENCES Employees(Id);')
        print('Added EmployeeId to RepairOrders')
    except Exception as e:
        print('Error on RepairOrders:', e)

    try:
        # Update existing employees with a random polish phone number if they don't have one
        cursor.execute('SELECT Id, PhoneNumber FROM Employees')
        employees = cursor.fetchall()
        for emp in employees:
            if not emp[1]:
                phone = int(f"48{random.randint(500, 899)}{random.randint(100, 999)}{random.randint(100, 999)}")
                cursor.execute('UPDATE Employees SET PhoneNumber = ? WHERE Id = ?', (phone, emp[0]))
        print('Updated existing employees with random phone numbers.')
    except Exception as e:
        print('Error updating phone numbers:', e)

    conn.commit()
    conn.close()

if __name__ == '__main__':
    run()
