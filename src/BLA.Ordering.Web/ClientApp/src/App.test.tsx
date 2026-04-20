import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import App from './App'

describe('App', () => {
  it('renders the primary heading', () => {
    render(<App />)

    expect(
      screen.getByRole('heading', {
        name: 'Get started',
      }),
    ).toBeInTheDocument()
  })

  it('increments counter when clicked', async () => {
    render(<App />)

    const button = screen.getByRole('button', {
      name: /Count is/i,
    })

    await userEvent.click(button)

    expect(button).toHaveTextContent('Count is 1')
  })
})
