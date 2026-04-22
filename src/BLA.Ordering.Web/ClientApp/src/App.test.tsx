import { render, screen } from '@testing-library/react'
import { within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import App from './App.tsx'

const fetchMock = vi.fn<typeof fetch>()

function jsonResponse(body: unknown, init?: ResponseInit) {
  return new Response(JSON.stringify(body), {
    status: 200,
    headers: {
      'Content-Type': 'application/json',
    },
    ...init,
  })
}

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)

  fetchMock.mockImplementation(async (input, init) => {
    const url = typeof input === 'string' ? input : input instanceof URL ? input.pathname : input.url

    if (url.endsWith('/api/auth/session')) {
      return jsonResponse(
        {
          isAuthenticated: false,
          userId: '',
          email: '',
          authenticationType: null,
        },
        { status: 401 },
      )
    }

    if (url.endsWith('/api/auth/login') && init?.method === 'POST') {
      return jsonResponse({
        userId: 7,
        email: 'admin@ballastlane.com',
        accessToken: 'jwt-token',
        expiresAtUtc: '2026-04-21T12:00:00Z',
        tokenType: 'Bearer',
      })
    }

    if (url.endsWith('/api/auth/logout') && init?.method === 'POST') {
      return new Response(null, { status: 204 })
    }

    throw new Error(`Unhandled fetch call: ${url}`)
  })
})

describe('App', () => {
  it('skips the login view when an authenticated MVC session already exists', async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        isAuthenticated: true,
        userId: '7',
        email: 'admin@ballastlane.com',
        authenticationType: 'Cookies',
      }),
    )

    render(<App />)

    expect(await screen.findByRole('heading', { name: 'Orders Dashboard' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Sign in to Orders Dashboard' })).not.toBeInTheDocument()
  })

  it('starts in authentication view and allows login', async () => {
    const user = userEvent.setup()
    render(<App />)

    expect(
      await screen.findByRole('heading', {
        name: 'Sign in to Orders Dashboard',
      }),
    ).toBeInTheDocument()

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))

    expect(await screen.findByRole('heading', { name: 'Orders Dashboard' })).toBeInTheDocument()
  })

  it('allows access to create, edit and delete order flows from dashboard', async () => {
    const user = userEvent.setup()
    render(<App />)
    await screen.findByRole('heading', { name: 'Sign in to Orders Dashboard' })

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))
    await screen.findByRole('heading', { name: 'Orders Dashboard' })

    await user.click(screen.getByRole('button', { name: 'Create Order' }))
    expect(screen.getByText('Create New Order')).toBeInTheDocument()

    await user.type(screen.getByLabelText('Customer ID *'), 'CUST900')
    await user.clear(screen.getByLabelText('Product Name 1'))
    await user.type(screen.getByLabelText('Product Name 1'), 'Cable')
    await user.clear(screen.getByLabelText('Quantity 1'))
    await user.type(screen.getByLabelText('Quantity 1'), '2')
    await user.clear(screen.getByLabelText('Unit Price 1'))
    await user.type(screen.getByLabelText('Unit Price 1'), '15')

    const createSection = screen.getByTestId('order-create')
    await user.click(within(createSection).getByRole('button', { name: 'Create Order' }))

    expect(screen.getByText('BL-2026-1003')).toBeInTheDocument()

    await user.click(screen.getAllByRole('button', { name: 'Edit' })[0])
    expect(screen.getByText(/Edit Order/)).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Save Changes' }))

    await user.click(screen.getAllByRole('button', { name: 'Delete' })[0])
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Delete Order' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('supports logout back to authentication screen', async () => {
    const user = userEvent.setup()
    render(<App />)
    await screen.findByRole('heading', { name: 'Sign in to Orders Dashboard' })

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))
    await screen.findByRole('heading', { name: 'Orders Dashboard' })
    await user.click(screen.getByRole('button', { name: 'Sign Out' }))

    expect(await screen.findByRole('heading', { name: 'Sign in to Orders Dashboard' })).toBeInTheDocument()
  })
})
