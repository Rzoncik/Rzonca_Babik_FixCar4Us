import sqlite3
conn = sqlite3.connect('database.db')
c = conn.cursor()
c.execute('SELECT Id, LoggedHours FROM OrderServices WHERE RepairOrderId = 1')
service = c.fetchone()
if service:
    c.execute('SELECT AdditionalFee FROM RepairOrders WHERE Id = 1')
    order = c.fetchone()
    additional_fee = order[0] if order and order[0] else 0.0
    c.execute('SELECT SUM(Quantity * PriceAtTheTime) FROM OrderParts WHERE RepairOrderId = 1')
    parts = c.fetchone()[0] or 0.0
    c.execute('SELECT BaseHourlyRate FROM Services WHERE Id = (SELECT ServiceId FROM OrderServices WHERE Id = ?)', (service[0],))
    svc = c.fetchone()
    base_rate = svc[0] if svc and svc[0] else 150.0
    final_price = parts + (0.5 * base_rate) + additional_fee
    c.execute('UPDATE OrderServices SET FinalPrice = ? WHERE Id = ?', (final_price, service[0]))
conn.commit()
conn.close()
