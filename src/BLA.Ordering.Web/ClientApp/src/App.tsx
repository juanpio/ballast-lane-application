import { useMemo, useState } from 'react'
import { OrderGrid, OrderTable } from './features/orders'
import type { OrderDto } from './features/orders'
import './App.css'

function App() {
  const [view, setView] = useState<'table' | 'grid'>('table')

  const orders = useMemo<OrderDto[]>(
    () => [
      {
        id: 'ord-1001',
        customerId: 'customer-001',
        orderNumber: 'BL-2026-1001',
        status: 'pending',
        totalAmount: 245.5,
        currency: 'USD',
        createdAt: '2026-04-20T14:20:00Z',
        updatedAt: '2026-04-20T14:20:00Z',
        items: [
          {
            id: 'item-1',
            productName: 'Wireless Mouse',
            quantity: 2,
            unitPrice: 49.9,
            totalPrice: 99.8,
          },
          {
            id: 'item-2',
            productName: 'Keyboard',
            quantity: 1,
            unitPrice: 145.7,
            totalPrice: 145.7,
          },
        ],
      },
      {
        id: 'ord-1002',
        customerId: 'customer-001',
        orderNumber: 'BL-2026-1002',
        status: 'shipped',
        totalAmount: 80,
        currency: 'EUR',
        createdAt: '2026-04-18T10:10:00Z',
        updatedAt: '2026-04-19T09:30:00Z',
        items: [
          {
            id: 'item-3',
            productName: 'USB-C Cable',
            quantity: 4,
            unitPrice: 20,
            totalPrice: 80,
          },
        ],
      },
    ],
    [],
  )

  return (
    <section className="dashboard">
      <header className="dashboard-header">
        <div>
          <h1>Orders Dashboard</h1>
          <p>Authenticated users can manage orders from this React micro-frontend.</p>
        </div>
        <div className="dashboard-actions" role="group" aria-label="Orders view selector">
          <button
            type="button"
            className={view === 'table' ? 'view-btn view-btn-active' : 'view-btn'}
            onClick={() => setView('table')}
          >
            Table view
          </button>
          <button
            type="button"
            className={view === 'grid' ? 'view-btn view-btn-active' : 'view-btn'}
            onClick={() => setView('grid')}
          >
            Grid view
          </button>
        </div>
      </header>

      <main className="dashboard-main">
        {view === 'table' ? (
          <OrderTable orders={orders} total={orders.length} page={1} pageSize={10} />
        ) : (
          <OrderGrid orders={orders} />
        )}
      </main>
    </section>
  )
}

export default App
