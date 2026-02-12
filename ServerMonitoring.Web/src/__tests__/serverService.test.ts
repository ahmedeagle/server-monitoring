import { describe, it, expect, vi, beforeEach } from 'vitest';
import { serverService } from '../services/serverService';
import api from '../services/api';

// Mock the api module
vi.mock('../services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn()
  }
}));

describe('ServerService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAll', () => {
    it('should fetch all servers', async () => {
      const mockServers = [
        { id: 1, name: 'Server 1', status: 'Online' },
        { id: 2, name: 'Server 2', status: 'Offline' }
      ];

      vi.mocked(api.get).mockResolvedValue({ data: mockServers });

      const result = await serverService.getAll();

      expect(api.get).toHaveBeenCalledWith('/v1/servers');
      expect(result).toEqual(mockServers);
    });
  });

  describe('getById', () => {
    it('should fetch server by ID', async () => {
      const mockServer = { id: 1, name: 'Test Server', status: 'Online' };
      
      vi.mocked(api.get).mockResolvedValue({ data: mockServer });

      const result = await serverService.getById(1);

      expect(api.get).toHaveBeenCalledWith('/v1/servers/1');
      expect(result).toEqual(mockServer);
    });
  });

  describe('create', () => {
    it('should create new server', async () => {
      const newServer = {
        name: 'New Server',
        hostname: 'new-server',
        ipAddress: '192.168.1.100',
        port: 22
      };
      const mockResponse = { data: { id: 1, ...newServer } };

      vi.mocked(api.post).mockResolvedValue(mockResponse);

      const result = await serverService.create(newServer);

      expect(api.post).toHaveBeenCalledWith('/v1/servers', newServer);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('update', () => {
    it('should update existing server', async () => {
      const updatedServer = {
        name: 'Updated Server',
        hostname: 'updated-server'
      };
      const mockResponse = { data: { id: 1, ...updatedServer } };
      
      vi.mocked(api.put).mockResolvedValue(mockResponse);

      const result = await serverService.update(1, updatedServer);

      expect(api.put).toHaveBeenCalledWith('/v1/servers/1', updatedServer);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('delete', () => {
    it('should delete server', async () => {
      vi.mocked(api.delete).mockResolvedValue({ data: null });

      await serverService.delete(1);

      expect(api.delete).toHaveBeenCalledWith('/v1/servers/1');
    });
  });
});
