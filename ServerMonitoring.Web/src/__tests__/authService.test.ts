import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../services/authService';
import api from '../services/api';

// Mock the api module
vi.mock('../services/api', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
  }
}));

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('login', () => {
    it('should login successfully and return auth data', async () => {
      const mockResponse = {
        data: {
          token: 'test-token',
          refreshToken: 'test-refresh-token',
          user: {
            username: 'testuser',
            email: 'test@example.com',
            roles: ['User']
          }
        }
      };

      vi.mocked(api.post).mockResolvedValue(mockResponse);

      const result = await authService.login({
        username: 'testuser',
        password: 'password123'
      });

      expect(api.post).toHaveBeenCalledWith('/v1/auth/login', {
        username: 'testuser',
        password: 'password123'
      });
      expect(result.token).toBe('test-token');
      expect(result.user.username).toBe('testuser');
    });

    it('should throw error on failed login', async () => {
      vi.mocked(api.post).mockRejectedValue(new Error('Invalid credentials'));

      await expect(
        authService.login({ username: 'wrong', password: 'wrong' })
      ).rejects.toThrow();
    });
  });

  describe('register', () => {
    it('should register successfully', async () => {
      const mockResponse = {
        data: {
          token: 'test-token',
          refreshToken: 'test-refresh-token',
          user: {
            username: 'newuser',
            email: 'new@example.com',
            roles: ['User']
          }
        }
      };

      vi.mocked(api.post).mockResolvedValue(mockResponse);

      const result = await authService.register({
        username: 'newuser',
        email: 'new@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User'
      });

      expect(api.post).toHaveBeenCalledWith('/v1/auth/register', {
        username: 'newuser',
        email: 'new@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User'
      });
      expect(result.token).toBe('test-token');
    });
  });
});
