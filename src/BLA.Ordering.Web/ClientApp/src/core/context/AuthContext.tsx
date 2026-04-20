import { createContext, useCallback, useContext, useMemo, useState } from 'react';

interface AuthContextValue {
	isAuthenticated: boolean;
	userName: string | null;
	isLoading: boolean;
	error: string | null;
	login: (email: string, password: string) => Promise<void>;
	logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
	children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
	const [isAuthenticated, setIsAuthenticated] = useState(false);
	const [userName, setUserName] = useState<string | null>(null);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const login = useCallback(async (email: string, password: string) => {
		setIsLoading(true);
		setError(null);

		try {
			if (!email.trim() || !password.trim()) {
				setError('Email and password are required');
				return;
			}

			setIsAuthenticated(true);
			setUserName(email.trim());
		} finally {
			setIsLoading(false);
		}
	}, []);

	const logout = useCallback(() => {
		setIsAuthenticated(false);
		setUserName(null);
		setError(null);
	}, []);

	const value = useMemo<AuthContextValue>(
		() => ({
			isAuthenticated,
			userName,
			isLoading,
			error,
			login,
			logout,
		}),
		[error, isAuthenticated, isLoading, login, logout, userName],
	);

	return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
	const context = useContext(AuthContext);
	if (!context) {
		throw new Error('useAuth must be used within an AuthProvider');
	}
	return context;
}
