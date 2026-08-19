import React, { useState, useEffect } from 'react';

export default function ProductFormModal({ isOpen, onClose, onSave, productToEdit }) {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (productToEdit) {
      setFormData({
        name: productToEdit.name || '',
        description: productToEdit.description || '',
        price: productToEdit.price !== undefined ? productToEdit.price.toString() : ''
      });
    } else {
      setFormData({
        name: '',
        description: '',
        price: ''
      });
    }
    setError('');
  }, [productToEdit, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const priceNum = parseFloat(formData.price);
    if (isNaN(priceNum) || priceNum <= 0) {
      setError('Price must be a valid number greater than 0.');
      return;
    }

    if (formData.name.trim().length < 3) {
      setError('Product name must be at least 3 characters.');
      return;
    }

    setLoading(true);
    try {
      await onSave({
        name: formData.name.trim(),
        description: formData.description.trim(),
        price: priceNum
      });
      onClose();
    } catch (err) {
      const errList = err.errors && err.errors.length > 0 ? err.errors.join(' ') : err.message;
      setError(errList || 'Failed to save product.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{productToEdit ? '✏️ Edit Product' : '➕ Add New Product'}</h2>
          <button className="btn-close" onClick={onClose}>✕</button>
        </div>

        {error && <div className="alert alert-danger">{error}</div>}

        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label>Product Name <span className="text-danger">*</span></label>
            <input 
              type="text" 
              className="input-field" 
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required 
              minLength={3}
              maxLength={100}
              placeholder="e.g. Ultra HD Smart Monitor"
            />
          </div>

          <div className="form-group">
            <label>Price (USD / $) <span className="text-danger">*</span></label>
            <input 
              type="number" 
              step="0.01"
              min="0.01"
              className="input-field" 
              value={formData.price}
              onChange={(e) => setFormData({ ...formData, price: e.target.value })}
              required 
              placeholder="e.g. 299.99"
            />
          </div>

          <div className="form-group">
            <label>Description <span className="text-danger">*</span></label>
            <textarea 
              className="input-field textarea-field" 
              rows={4}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              required 
              maxLength={1000}
              placeholder="Provide detailed product specifications and features..."
            />
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn-outline" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : (productToEdit ? 'Save Changes' : 'Create Product')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
