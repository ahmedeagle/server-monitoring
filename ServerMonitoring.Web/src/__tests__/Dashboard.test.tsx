import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Dashboard from '../pages/Dashboard';
import { serverService } from '../services/serverService';
import { metricService } from '../services/metricService';

vi.mock('../services/serverService');
vi.mock('../services/metricService');

describe('Dashboard Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    vi.mocked(serverService.getServers).mockResolvedValue({
      data: [
        { id: 1, name: 'Server 1', status: 'Up', ipAddress: '192.168.1.1' },
        { id: 2, name: 'Server 2', status: 'Down', ipAddress: '192.168.1.2' },
      ],
      total: 2,
    });

    vi.mocked(metricService.getRecentMetrics).mockResolvedValue([
      {
        id: 1,
        serverId: 1,
        cpuUsage: 45.5,
        memoryUsage: 60.2,
        diskUsage: 70.1,
        responseTime: 120,
        timestamp: new Date().toISOString(),
      },
    ]);
  });

  it('should render dashboard title', () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    expect(screen.getByText(/dashboard/i)).toBeInTheDocument();
  });

  it('should display loading state initially', () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should load and display server data', async () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(serverService.getServers).toHaveBeenCalled();
    });

    expect(screen.getByText(/server 1/i)).toBeInTheDocument();
    expect(screen.getByText(/server 2/i)).toBeInTheDocument();
  });

  it('should display server status correctly', async () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/up/i)).toBeInTheDocument();
      expect(screen.getByText(/down/i)).toBeInTheDocument();
    });
  });

  it('should load metrics data', async () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(metricService.getRecentMetrics).toHaveBeenCalled();
    });
  });
});
