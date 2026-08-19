import React, { useState, useEffect, useCallback } from 'react';
import { api, tokenStorage } from './api/apiService';
import Navbar from './components/Navbar';
import AuthModal from './components/AuthModal';
import ProductFormModal from './components/ProductFormModal';
import FilterBar from './components/FilterBar';
import ProductCard from './components/ProductCard';
import Pagination from './components/Pagination';
import Toast from './components/Toast';

export default function App() {
  const [user, setUser] = useState(tokenStorage.getUser());
  const [products, setProducts] = useState([]);
  const [pagination, setPagination] = useState({ page: 1, totalPages: 1, totalCount: 0 });
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);

  // Filters State
  const [filters, setFilters] = useState({
    keyword: '',
    minPrice: '',
    maxPrice: '',
    sortBy: 'CreatedAt',
    sortDescending: true,
    page: 1,
    pageSize: 6
  });

  // Modal States
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
  const [isProductModalOpen, setIsProductModalOpen] = useState(false);
  const [productToEdit, setProductToEdit] = useState(null);

  const showToast = (message, type = 'success') => {
    setToast({ message, type });
  };

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.products.getAll(filters);
      if (res.success && res.data) {
        setProducts(res.data.items);
        setPagination({
          page: res.data.page,
          totalPages: res.data.totalPages,
          totalCount: res.data.totalCount
        });
      }
    } catch (err) {
      showToast(err.message || 'Failed to load products.', 'error');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  // Auth Handlers
  const handleLogin = async (usernameOrEmail, password) => {
    const res = await api.auth.login(usernameOrEmail, password);
    setUser(tokenStorage.getUser());
    showToast(`Welcome back, ${res.data.username}!`, 'success');
  };

  const handleRegister = async (username, email, password) => {
    const res = await api.auth.register(username, email, password);
    setUser(tokenStorage.getUser());
    showToast(`Registration successful. Welcome, ${res.data.username}!`, 'success');
  };

  const handleLogout = () => {
    api.auth.logout();
    setUser(null);
    showToast('Logged out successfully.', 'success');
  };

  // Product CRUD Handlers
  const handleOpenAddProduct = () => {
    if (!user) {
      showToast('Please login first to create products.', 'error');
      setIsAuthModalOpen(true);
      return;
    }
    setProductToEdit(null);
    setIsProductModalOpen(true);
  };

  const handleOpenEditProduct = (product) => {
    if (!user) {
      showToast('Please login first to edit products.', 'error');
      setIsAuthModalOpen(true);
      return;
    }
    setProductToEdit(product);
    setIsProductModalOpen(true);
  };

  const handleSaveProduct = async (productData) => {
    if (productToEdit) {
      await api.products.update(productToEdit.id, productData);
      showToast('Product updated successfully!', 'success');
    } else {
      await api.products.create(productData);
      showToast('Product created successfully!', 'success');
    }
    fetchProducts();
  };

  const handleDeleteProduct = async (id, name) => {
    if (!user) {
      showToast('Please login first to delete products.', 'error');
      setIsAuthModalOpen(true);
      return;
    }

    if (window.confirm(`Are you sure you want to delete "${name}"?`)) {
      try {
        await api.products.delete(id);
        showToast(`Product "${name}" deleted successfully!`, 'success');
        fetchProducts();
      } catch (err) {
        showToast(err.message || 'Failed to delete product.', 'error');
      }
    }
  };

  // Filter Handlers
  const handleFilterChange = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value, page: 1 }));
  };

  const handleResetFilters = () => {
    setFilters({
      keyword: '',
      minPrice: '',
      maxPrice: '',
      sortBy: 'CreatedAt',
      sortDescending: true,
      page: 1,
      pageSize: 6
    });
  };

  const handlePageChange = (newPage) => {
    setFilters(prev => ({ ...prev, page: newPage }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="app-container">
      <Navbar 
        user={user}
        onOpenAuth={() => setIsAuthModalOpen(true)}
        onLogout={handleLogout}
        onOpenAddProduct={handleOpenAddProduct}
      />

      <main className="main-content">
        {/* Banner Section */}
        <section className="hero-banner">
          <div className="hero-content">
            <h1>Product Management Platform</h1>
            <p>Manage, search, filter, and organize your product catalog with secure JWT-protected REST APIs.</p>
          </div>
        </section>

        {/* Filter Section */}
        <FilterBar 
          filters={filters}
          onFilterChange={handleFilterChange}
          onSearch={fetchProducts}
          onReset={handleResetFilters}
        />

        {/* Product List Section */}
        <section className="catalog-section">
          <div className="catalog-header">
            <h2>Product Catalog</h2>
            <span className="badge-count">{pagination.totalCount} Products Found</span>
          </div>

          {loading ? (
            <div className="loading-state">
              <div className="spinner"></div>
              <p>Loading products...</p>
            </div>
          ) : products.length === 0 ? (
            <div className="empty-state">
              <span className="empty-icon">📂</span>
              <h3>No products found</h3>
              <p>Try adjusting your search keywords or price filters.</p>
              <button className="btn btn-outline" onClick={handleResetFilters}>
                Clear All Filters
              </button>
            </div>
          ) : (
            <div className="products-grid">
              {products.map(product => (
                <ProductCard 
                  key={product.id}
                  product={product}
                  user={user}
                  onEdit={handleOpenEditProduct}
                  onDelete={handleDeleteProduct}
                />
              ))}
            </div>
          )}

          {/* Pagination */}
          <Pagination 
            page={pagination.page}
            totalPages={pagination.totalPages}
            totalCount={pagination.totalCount}
            onPageChange={handlePageChange}
          />
        </section>
      </main>

      {/* Modals */}
      <AuthModal 
        isOpen={isAuthModalOpen}
        onClose={() => setIsAuthModalOpen(false)}
        onLoginSuccess={handleLogin}
        onRegisterSuccess={handleRegister}
      />

      <ProductFormModal 
        isOpen={isProductModalOpen}
        onClose={() => setIsProductModalOpen(false)}
        onSave={handleSaveProduct}
        productToEdit={productToEdit}
      />

      {/* Toast Notification */}
      <Toast toast={toast} onClose={() => setToast(null)} />
    </div>
  );
}
