import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import ServersPage from '../pages/ServersPage';
import { serverService } from '../services/serverService';

vi.mock('../services/serverService', () => ({
  serverService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}));

describe('ServerList Component', () => {
  const mockServers = [
    {
      id: 1,
      name: 'Web Server 01',
      hostname: 'web-01',
      ipAddress: '192.168.1.100',
      port: 8080,
      status: 'Up',
      operatingSystem: 'Windows Server 2022',
      isActive: true,
    },
    {
      id: 2,
      name: 'Database Server',
      hostname: 'db-01',
      ipAddress: '192.168.1.101',
      port: 5432,
      status: 'Down',
      operatingSystem: 'Ubuntu 22.04',
      isActive: true,
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(serverService.getAll).mockResolvedValue(mockServers);
  });

  it('should render server list', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
      expect(screen.getByText('Database Server')).toBeInTheDocument();
    });
  });

  it('should load and display servers', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(serverService.getAll).toHaveBeenCalled();
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
    });
  });

  it('should call getAll on mount', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(serverService.getAll).toHaveBeenCalled();
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
      expect(screen.getByText('Database Server')).toBeInTheDocument();
    });
  });

  it('should render servers table', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
      expect(screen.getByText('web-01')).toBeInTheDocument();
      expect(screen.getByText('192.168.1.100')).toBeInTheDocument();
    });
  });

  it('should display server details', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
      expect(screen.getByText('Database Server')).toBeInTheDocument();
      expect(screen.getByText('web-01')).toBeInTheDocument();
      expect(screen.getByText('db-01')).toBeInTheDocument();
    });
  });
});
