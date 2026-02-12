import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import DashboardPage from '../pages/DashboardPage';
import { serverService } from '../services/serverService';
import { alertService } from '../services/alertService';

vi.mock('../services/serverService', () => ({
  serverService: {
    getAll: vi.fn(),
    getMetrics: vi.fn()
  }
}));

vi.mock('../services/alertService', () => ({
  alertService: {
    getAll: vi.fn()
  }
}));

vi.mock('../contexts/SignalRContext', () => ({
  useSignalR: () => ({
    connection: null,
    isConnected: false
  })
}));

describe('Dashboard Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    vi.mocked(serverService.getAll).mockResolvedValue([
      { id: 1, name: 'Server 1', status: 'Online', ipAddress: '192.168.1.1', hostname: 'server1', port: 22, operatingSystem: 'Linux', isActive: true },
      { id: 2, name: 'Server 2', status: 'Offline', ipAddress: '192.168.1.2', hostname: 'server2', port: 22, operatingSystem: 'Linux', isActive: true },
    ]);

    vi.mocked(alertService.getAll).mockResolvedValue([]);
    vi.mocked(serverService.getMetrics).mockResolvedValue([]);
  });

  it('should render dashboard component', async () => {
    render(
      <BrowserRouter>
        <DashboardPage />
      </BrowserRouter>
    );

    expect(await screen.findByText(/dashboard/i)).toBeInTheDocument();
  });
});
