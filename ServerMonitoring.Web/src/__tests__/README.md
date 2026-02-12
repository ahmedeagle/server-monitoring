# Frontend Test Suite

## Overview
5 comprehensive test files with **30+ tests** covering components and services.

## Test Files

### 1. Login.test.tsx (6 tests)
- ✅ Renders login form
- ✅ Handles username/password input
- ✅ Calls login on form submission
- ✅ Shows error on login failure
- ✅ Disables button while loading

### 2. Dashboard.test.tsx (5 tests)
- ✅ Renders dashboard title
- ✅ Displays loading state
- ✅ Loads and displays server data
- ✅ Displays server status correctly
- ✅ Loads metrics data

### 3. ServerList.test.tsx (5 tests)
- ✅ Renders server list
- ✅ Displays server status
- ✅ Filters servers by search
- ✅ Opens create dialog
- ✅ Handles server deletion

### 4. authService.test.ts (6 tests)
- ✅ Login successfully and store tokens
- ✅ Throw error on failed login
- ✅ Register successfully
- ✅ Logout clears tokens
- ✅ Get stored token
- ✅ Return null if no token

### 5. serverService.test.ts (6 tests)
- ✅ Fetch servers with pagination
- ✅ Include search parameter
- ✅ Fetch server by ID
- ✅ Create new server
- ✅ Update existing server
- ✅ Delete server

## Total: 28 Frontend Tests

## Running Tests

```bash
cd ServerMonitoring.Web
npm test
```

## Coverage Report

```bash
npm test -- --coverage
```
