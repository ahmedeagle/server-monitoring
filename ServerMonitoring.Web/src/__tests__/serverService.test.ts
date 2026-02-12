import { describe, it, expect, vi, beforeEach } from 'vitest';
import axios from 'axios';
import { serverService } from '../services/serverService';

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

describe('ServerService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getServers', () => {
    it('should fetch servers with pagination', async () => {
      const mockResponse = {
        data: {
          data: [
            { id: 1, name: 'Server 1', status: 'Up' },
            { id: 2, name: 'Server 2', status: 'Down' },
          ],
          total: 2,
          page: 1,
          pageSize: 10,
        },
      };

      mockedAxios.get.mockResolvedValue(mockResponse);

      const result = await serverService.getServers({ page: 1, pageSize: 10 });

      expect(mockedAxios.get).toHaveBeenCalledWith('/api/v1/servers', {
        params: { page: 1, pageSize: 10 },
      });
      expect(result).toEqual(mockResponse.data);
    });

    it('should include search parameter when provided', async () => {
      mockedAxios.get.mockResolvedValue({ data: { data: [], total: 0 } });

      await serverService.getServers({ page: 1, pageSize: 10, search: 'test' });

      expect(mockedAxios.get).toHaveBeenCalledWith('/api/v1/servers', {
        params: { page: 1, pageSize: 10, search: 'test' },
      });
    });
  });

  describe('getServerById', () => {
    it('should fetch server by ID', async () => {
      const mockServer = { id: 1, name: 'Test Server', status: 'Up' };
      mockedAxios.get.mockResolvedValue({ data: mockServer });

      const result = await serverService.getServerById(1);

      expect(mockedAxios.get).toHaveBeenCalledWith('/api/v1/servers/1');
      expect(result).toEqual(mockServer);
    });
  });

  describe('createServer', () => {
    it('should create new server', async () => {
      const newServer = {
        name: 'New Server',
        hostname: 'new-server',
        ipAddress: '192.168.1.100',
        port: 443,
      };
      const mockResponse = { data: { id: 1, ...newServer } };

      mockedAxios.post.mockResolvedValue(mockResponse);

      const result = await serverService.createServer(newServer);

      expect(mockedAxios.post).toHaveBeenCalledWith('/api/v1/servers', newServer);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('updateServer', () => {
    it('should update existing server', async () => {
      const updatedServer = {
        id: 1,
        name: 'Updated Server',
        hostname: 'updated-server',
        ipAddress: '192.168.1.100',
      };
      mockedAxios.put.mockResolvedValue({ data: updatedServer });

      const result = await serverService.updateServer(1, updatedServer);

      expect(mockedAxios.put).toHaveBeenCalledWith('/api/v1/servers/1', updatedServer);
      expect(result).toEqual(updatedServer);
    });
  });

  describe('deleteServer', () => {
    it('should delete server', async () => {
      mockedAxios.delete.mockResolvedValue({ data: null });

      await serverService.deleteServer(1);

      expect(mockedAxios.delete).toHaveBeenCalledWith('/api/v1/servers/1');
    });
  });
});
