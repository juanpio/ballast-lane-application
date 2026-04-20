import { render, screen } from '@testing-library/react'
import { within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import App from './App.tsx'

describe('App', () => {
  it('starts in authentication view and allows login', async () => {
    const user = userEvent.setup()
    render(<App />)

    expect(
      screen.getByRole('heading', {
        name: 'Sign in to Orders Dashboard',
      }),
    ).toBeInTheDocument()

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))

    expect(screen.getByRole('heading', { name: 'Orders Dashboard' })).toBeInTheDocument()
  })

  it('allows access to create, edit and delete order flows from dashboard', async () => {
    const user = userEvent.setup()
    render(<App />)

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))

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

    await user.type(screen.getByLabelText('Email'), 'admin@ballastlane.com')
    await user.type(screen.getByLabelText('Password'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Sign In' }))
    await user.click(screen.getByRole('button', { name: 'Sign Out' }))

    expect(screen.getByRole('heading', { name: 'Sign in to Orders Dashboard' })).toBeInTheDocument()
  })
})
