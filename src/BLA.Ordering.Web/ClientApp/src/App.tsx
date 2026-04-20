import { useMemo, useState } from 'react'
import { AuthProvider, useAuth } from './core/context/AuthContext'
import { LoginForm } from './features/auth/components/LoginForm'
import { OrderCreate, OrderDeleteDialog, OrderEdit, OrderTable } from './features/orders'
import type { OrderDto } from './features/orders'
import './App.css'

const initialOrders: OrderDto[] = [
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
]

function DashboardShell() {
  const [view, setView] = useState<'table' | 'create' | 'edit'>('table')
  const [orders, setOrders] = useState<OrderDto[]>(initialOrders)
  const [editingOrder, setEditingOrder] = useState<OrderDto | null>(null)
  const [deletingOrder, setDeletingOrder] = useState<OrderDto | null>(null)
  const { userName, logout } = useAuth()

  const total = orders.length

  const nextOrderNumber = useMemo(() => {
    const highest = orders.reduce((max, order) => {
      const suffix = Number(order.orderNumber.split('-').pop())
      if (Number.isNaN(suffix)) {
        return max
      }
      return Math.max(max, suffix)
    }, 1000)

    return (highest + 1).toString().padStart(4, '0')
  }, [orders])

  return (
    <section className="dashboard">
      <header className="dashboard-header">
        <div>
          <h1>Orders Dashboard</h1>
          <p>{`Authenticated as ${userName ?? 'unknown user'}. Manage orders from this dashboard.`}</p>
        </div>
        <div className="dashboard-actions" role="group" aria-label="Orders actions">
          <button
            type="button"
            className={view === 'table' ? 'view-btn view-btn-active' : 'view-btn'}
            onClick={() => setView('table')}
          >
            View Orders
          </button>
          <button
            type="button"
            className={view === 'create' ? 'view-btn view-btn-active' : 'view-btn'}
            onClick={() => setView('create')}
          >
            Create Order
          </button>
          <button type="button" className="view-btn" onClick={logout}>
            Sign Out
          </button>
        </div>
      </header>

      <main className="dashboard-main">
        {view === 'table' ? (
          <OrderTable
            orders={orders}
            total={total}
            page={1}
            pageSize={10}
            onEditOrder={(order) => {
              setEditingOrder(order)
              setView('edit')
            }}
            onDeleteOrder={(orderId) => {
              const selected = orders.find((item) => item.id === orderId) ?? null
              setDeletingOrder(selected)
            }}
          />
        ) : null}

        {view === 'create' ? (
          <OrderCreate
            onCancel={() => setView('table')}
            onSubmit={async (data) => {
              const now = new Date().toISOString()
              const totalAmount = data.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)

              const newOrder: OrderDto = {
                id: `ord-${crypto.randomUUID()}`,
                customerId: data.customerId,
                orderNumber: `BL-2026-${nextOrderNumber}`,
                status: 'pending',
                totalAmount,
                currency: 'USD',
                createdAt: now,
                updatedAt: now,
                items: data.items.map((item) => ({
                  id: crypto.randomUUID(),
                  productName: item.productName,
                  quantity: item.quantity,
                  unitPrice: item.unitPrice,
                  totalPrice: item.quantity * item.unitPrice,
                })),
              }

              setOrders((current) => [...current, newOrder])
              setView('table')
            }}
          />
        ) : null}

        {view === 'edit' ? (
          <OrderEdit
            order={editingOrder}
            onCancel={() => {
              setEditingOrder(null)
              setView('table')
            }}
            onSubmit={async (data) => {
              setOrders((current) =>
                current.map((order) => {
                  if (order.id !== data.id) {
                    return order
                  }

                  const items =
                    data.items?.map((item) => ({
                      id: crypto.randomUUID(),
                      productName: item.productName,
                      quantity: item.quantity,
                      unitPrice: item.unitPrice,
                      totalPrice: item.quantity * item.unitPrice,
                    })) ?? order.items

                  return {
                    ...order,
                    status: data.status ?? order.status,
                    items,
                    totalAmount: items.reduce((sum, item) => sum + item.totalPrice, 0),
                    updatedAt: new Date().toISOString(),
                  }
                }),
              )

              setEditingOrder(null)
              setView('table')
            }}
          />
        ) : null}
      </main>

      <OrderDeleteDialog
        isOpen={Boolean(deletingOrder)}
        order={deletingOrder}
        onCancel={() => setDeletingOrder(null)}
        onConfirm={async (orderId) => {
          setOrders((current) => current.filter((order) => order.id !== orderId))
          setDeletingOrder(null)
          if (view !== 'table') {
            setView('table')
          }
        }}
      />
    </section>
  )
}

function AppShell() {
  const { isAuthenticated, isLoading, error, login } = useAuth()

  if (!isAuthenticated) {
    return <LoginForm isLoading={isLoading} error={error} onSubmit={login} />
  }

  return <DashboardShell />
}

function App() {
  return (
    <AuthProvider>
      <AppShell />
    </AuthProvider>
  )
}

export default App
