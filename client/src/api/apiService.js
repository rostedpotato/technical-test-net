const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5187/api';

export const tokenStorage = {
  getToken: () => localStorage.getItem('pm_token'),
  setToken: (token) => localStorage.setItem('pm_token', token),
  removeToken: () => {
    localStorage.removeItem('pm_token');
    localStorage.removeItem('pm_user');
  },
  getUser: () => {
    const userStr = localStorage.getItem('pm_user');
    try {
      return userStr ? JSON.parse(userStr) : null;
    } catch {
      return null;
    }
  },
  setUser: (user) => localStorage.setItem('pm_user', JSON.stringify(user))
};

async function request(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };

  const token = tokenStorage.getToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers
  });

  let data;
  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    data = await response.json();
  } else {
    data = await response.text();
  }

  if (!response.ok) {
    const errorMessage = data?.message || (typeof data === 'string' ? data : 'An error occurred.');
    const errorDetails = data?.errors || [];
    const error = new Error(errorMessage);
    error.status = response.status;
    error.errors = Array.isArray(errorDetails) ? errorDetails : [JSON.stringify(errorDetails)];
    throw error;
  }

  return data;
}

export const api = {
  auth: {
    login: async (usernameOrEmail, password) => {
      const res = await request('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ usernameOrEmail, password })
      });
      if (res.success && res.data) {
        tokenStorage.setToken(res.data.token);
        tokenStorage.setUser({
          username: res.data.username,
          email: res.data.email,
          role: res.data.role
        });
      }
      return res;
    },
    register: async (username, email, password) => {
      const res = await request('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password })
      });
      if (res.success && res.data) {
        tokenStorage.setToken(res.data.token);
        tokenStorage.setUser({
          username: res.data.username,
          email: res.data.email,
          role: res.data.role
        });
      }
      return res;
    },
    getMe: async () => {
      return await request('/auth/me');
    },
    logout: () => {
      tokenStorage.removeToken();
    }
  },
  products: {
    getAll: async (params = {}) => {
      const query = new URLSearchParams();
      if (params.keyword) query.append('keyword', params.keyword);
      if (params.minPrice !== undefined && params.minPrice !== '') query.append('minPrice', params.minPrice);
      if (params.maxPrice !== undefined && params.maxPrice !== '') query.append('maxPrice', params.maxPrice);
      if (params.page) query.append('page', params.page);
      if (params.pageSize) query.append('pageSize', params.pageSize);
      if (params.sortBy) query.append('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) query.append('sortDescending', params.sortDescending);

      const queryString = query.toString();
      return await request(`/products${queryString ? `?${queryString}` : ''}`);
    },
    getById: async (id) => {
      return await request(`/products/${id}`);
    },
    create: async (productData) => {
      return await request('/products', {
        method: 'POST',
        body: JSON.stringify(productData)
      });
    },
    update: async (id, productData) => {
      return await request(`/products/${id}`, {
        method: 'PUT',
        body: JSON.stringify(productData)
      });
    },
    delete: async (id) => {
      return await request(`/products/${id}`, {
        method: 'DELETE'
      });
    }
  }
};
