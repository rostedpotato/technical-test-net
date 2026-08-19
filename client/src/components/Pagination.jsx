import React from 'react';

export default function Pagination({ page, totalPages, totalCount, onPageChange }) {
  if (totalPages <= 1) return null;

  return (
    <div className="pagination-container">
      <span className="pagination-info">
        Showing Page <strong>{page}</strong> of <strong>{totalPages}</strong> ({totalCount} items)
      </span>
      <div className="pagination-buttons">
        <button
          className="btn btn-outline btn-sm"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          ◀ Previous
        </button>

        {Array.from({ length: totalPages }, (_, i) => i + 1).map((pageNum) => (
          <button
            key={pageNum}
            className={`btn btn-sm ${page === pageNum ? 'btn-primary' : 'btn-outline'}`}
            onClick={() => onPageChange(pageNum)}
          >
            {pageNum}
          </button>
        ))}

        <button
          className="btn btn-outline btn-sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next ▶
        </button>
      </div>
    </div>
  );
}
