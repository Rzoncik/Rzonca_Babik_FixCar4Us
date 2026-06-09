import sqlite3
conn = sqlite3.connect('database.db')
c = conn.cursor()
c.execute("INSERT INTO OrderParts (Id, RepairOrderId, PartId, Quantity, PriceAtTheTime) VALUES (1, 1, 1, 2, 40.0)")
conn.commit()
conn.close()
