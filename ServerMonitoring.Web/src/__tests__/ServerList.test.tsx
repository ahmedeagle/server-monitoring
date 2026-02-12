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
      status: 'Up',
      operatingSystem: 'Windows Server 2022',
      isActive: true,
    },
    {
      id: 2,
      name: 'Database Server',
      hostname: 'db-01',
      ipAddress: '192.168.1.101',
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

  it('should display server status', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Up')).toBeInTheDocument();
      expect(screen.getByText('Down')).toBeInTheDocument();
    });
  });

  it('should filter servers by search term', async () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText(/search/i);
    fireEvent.change(searchInput, { target: { value: 'Database' } });

    await waitFor(() => {
      expect(serverService.getServers).toHaveBeenCalledWith(
        expect.objectContaining({ search: 'Database' })
      );
    });
  });

  it('should open create dialog', () => {
    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    const createButton = screen.getByRole('button', { name: /add server/i });
    fireEvent.click(createButton);

    expect(screen.getByText(/create new server/i)).toBeInTheDocument();
  });

  it('should handle server deletion', async () => {
    vi.mocked(serverService.delete).mockResolvedValue();

    render(
      <BrowserRouter>
        <ServersPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Web Server 01')).toBeInTheDocument();
    });

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    fireEvent.click(deleteButtons[0]);

    // Confirm deletion
    const confirmButton = screen.getByRole('button', { name: /confirm/i });
    fireEvent.click(confirmButton);

    await waitFor(() => {
      expect(serverService.deleteServer).toHaveBeenCalledWith(1);
    });
  });
});
