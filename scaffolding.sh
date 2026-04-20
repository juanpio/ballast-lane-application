#!/usr/bin/env bash

set -euo pipefail

# Generate a script to scaffold a new project the sln name is BLA.Ordering
# Initialize the solution
dotnet new sln -n BLA.Ordering

# Create Core and Infrastructure layers
dotnet new classlib -o src/BLA.Ordering.Domain
dotnet new classlib -o src/BLA.Ordering.Application
dotnet new classlib -o src/BLA.Ordering.Infrastructure

# Create the Web entry point (ASP.NET Core)
dotnet new web -o src/BLA.Ordering.Web

# Create Test projects
dotnet new xunit -o tests/UnitTests/BLA.Ordering.Application.Tests
dotnet new xunit -o tests/IntegrationTests/BLA.Ordering.Web.API.Tests

# Add projects to solution
dotnet sln add src/BLA.Ordering.Domain/BLA.Ordering.Domain.csproj
dotnet sln add src/BLA.Ordering.Application/BLA.Ordering.Application.csproj
dotnet sln add src/BLA.Ordering.Infrastructure/BLA.Ordering.Infrastructure.csproj
dotnet sln add src/BLA.Ordering.Web/BLA.Ordering.Web.csproj
dotnet sln add tests/UnitTests/BLA.Ordering.Application.Tests/BLA.Ordering.Application.Tests.csproj
dotnet sln add tests/IntegrationTests/BLA.Ordering.Web.API.Tests/BLA.Ordering.Web.API.Tests.csproj

# Add project references
dotnet add src/BLA.Ordering.Application/BLA.Ordering.Application.csproj reference src/BLA.Ordering.Domain/BLA.Ordering.Domain.csproj
dotnet add src/BLA.Ordering.Infrastructure/BLA.Ordering.Infrastructure.csproj reference src/BLA.Ordering.Domain/BLA.Ordering.Domain.csproj
dotnet add src/BLA.Ordering.Web/BLA.Ordering.Web.csproj reference src/BLA.Ordering.Application/BLA.Ordering.Application.csproj
dotnet add src/BLA.Ordering.Web/BLA.Ordering.Web.csproj reference src/BLA.Ordering.Infrastructure/BLA.Ordering.Infrastructure.csproj

# Add the React app to the Web project using Vite + TypeScript, Tailwind, Storybook and Vitest
cd src/BLA.Ordering.Web
printf 'n\n' | npx --yes create-vite@latest clientapp --template react-ts
mv clientapp ClientApp
cd ClientApp

# Install Vite template dependencies
npm install

# Core runtime deps
npm install react-router-dom web-vitals

# Styling and build deps
npm install -D tailwindcss postcss autoprefixer

# Testing deps
npm install -D vitest jsdom @vitest/coverage-v8 @testing-library/react @testing-library/jest-dom @testing-library/user-event @axe-core/react eslint-plugin-jsx-a11y

# Storybook packages (non-interactive)
npm install -D storybook @storybook/react-vite @storybook/addon-a11y @storybook/addon-docs @storybook/addon-onboarding @storybook/addon-vitest eslint-plugin-storybook

# Create target frontend structure
mkdir -p src/apps/auth src/apps/dashboard
mkdir -p src/core/api src/core/context src/core/services
mkdir -p src/features/auth/components src/features/auth/hooks src/features/auth/stories src/features/auth/__tests__
mkdir -p src/features/orders/components src/features/orders/hooks src/features/orders/skeletons src/features/orders/stories src/features/orders/__tests__
mkdir -p src/shared/components src/shared/styles src/shared/utils
mkdir -p public/locales

# Add scaffold files
touch src/apps/auth/main.tsx src/apps/dashboard/main.tsx
touch src/core/api/apiClient.ts src/core/context/AuthContext.tsx
touch src/core/services/authService.ts src/core/services/orderService.ts
touch src/features/auth/components/LoginForm.tsx src/features/auth/stories/LoginForm.stories.tsx src/features/auth/hooks/useAuthActions.ts
touch src/features/orders/components/OrderList.tsx src/features/orders/stories/OrderList.stories.tsx src/features/orders/skeletons/OrderSkeleton.tsx src/features/orders/hooks/useOrders.ts
touch src/shared/components/Button.tsx src/shared/components/Modal.tsx src/shared/components/Loading.tsx src/shared/components/InputControl.tsx src/shared/components/CheckControl.tsx
touch src/shared/styles/tailwind.css public/locales/en.json

# Configure Tailwind content paths
cat > tailwind.config.js <<'EOF'
module.exports = {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {},
  },
  plugins: [],
};
EOF

cd ../../..