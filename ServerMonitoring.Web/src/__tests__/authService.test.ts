import { describe, it, expect, vi, beforeEach } from 'vitest';
import axios from 'axios';
import { authService } from '../services/authService';

// Mock axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      interceptors: {
        request: { use: vi.fn(), eject: vi.fn() },
        response: { use: vi.fn(), eject: vi.fn() }
      },
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn()
    }))
  }
}));

const mockedAxios = vi.mocked(axios, true);

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  describe('login', () => {
    it('should login successfully and store tokens', async () => {
      const mockResponse = {
        data: {
          token: 'test-token',
          refreshToken: 'test-refresh-token',
          user: {
            id: 1,
            username: 'testuser',
            email: 'test@example.com',
          },
        },
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      const result = await authService.login({
        username: 'testuser',
        password: 'password123',
      });

      expect(mockedAxios.post).toHaveBeenCalledWith('/api/v1/auth/login', {
        username: 'testuser',
        password: 'password123',
      });
      expect(result).toEqual(mockResponse.data);
      expect(localStorage.getItem('token')).toBe('test-token');
      expect(localStorage.getItem('refreshToken')).toBe('test-refresh-token');
    });

    it('should throw error on failed login', async () => {
      mockedAxios.post.mockRejectedValue(new Error('Invalid credentials'));

      await expect(
        authService.login({ username: 'wrong', password: 'wrong' })
      ).rejects.toThrow('Invalid credentials');
    });
  });

  describe('register', () => {
    it('should register successfully', async () => {
      const mockResponse = {
        data: {
          token: 'new-token',
          refreshToken: 'new-refresh-token',
          user: {
            id: 2,
            username: 'newuser',
            email: 'new@example.com',
          },
        },
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      const result = await authService.register({
        username: 'newuser',
        email: 'new@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User',
      });

      expect(mockedAxios.post).toHaveBeenCalledWith('/api/v1/auth/register', {
        username: 'newuser',
        email: 'new@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User',
      });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('logout', () => {
    it('should clear tokens from localStorage', () => {
      localStorage.setItem('token', 'test-token');
      localStorage.setItem('refreshToken', 'test-refresh-token');

      authService.logout();

      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('refreshToken')).toBeNull();
    });
  });

  describe('getToken', () => {
    it('should return stored token', () => {
      localStorage.setItem('token', 'stored-token');
      
      const token = authService.getToken();
      
      expect(token).toBe('stored-token');
    });

    it('should return null if no token', () => {
      const token = authService.getToken();
      
      expect(token).toBeNull();
    });
  });
});
