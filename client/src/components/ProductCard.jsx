import React from 'react';

export default function ProductCard({ product, user, onEdit, onDelete }) {
  const isAdmin = user?.role?.toLowerCase() === 'admin';
  const formattedPrice = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(product.price);

  const formattedDate = new Date(product.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });

  return (
    <div className="product-card">
      <div className="product-card-header">
        <h3 className="product-title">{product.name}</h3>
        <span className="product-price">{formattedPrice}</span>
      </div>

      <p className="product-description">{product.description}</p>

      <div className="product-card-footer">
        <div className="product-meta">
          <span className="meta-icon">📅</span>
          <span className="meta-text">Added {formattedDate}</span>
        </div>

        {isAdmin && (
          <div className="card-actions">
            <button
              className="btn btn-sm btn-edit"
              onClick={() => onEdit(product)}
              title="Edit product"
            >
              ✏️ Edit
            </button>
            <button
              className="btn btn-sm btn-delete"
              onClick={() => onDelete(product.id, product.name)}
              title="Delete product"
            >
              🗑️ Delete
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
