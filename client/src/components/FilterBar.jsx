import React from 'react';

export default function FilterBar({ filters, onFilterChange, onSearch, onReset }) {
  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      onSearch();
    }
  };

  return (
    <div className="filter-card">
      <div className="filter-grid">
        {/* Keyword Search */}
        <div className="filter-item filter-search">
          <label className="filter-label">Search Product</label>
          <div className="search-input-wrapper">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              className="input-field search-input"
              placeholder="Search by name or description..."
              value={filters.keyword}
              onChange={(e) => onFilterChange('keyword', e.target.value)}
              onKeyDown={handleKeyDown}
            />
          </div>
        </div>

        {/* Min Price */}
        <div className="filter-item">
          <label className="filter-label">Min Price ($)</label>
          <input
            type="number"
            min="0"
            step="1"
            className="input-field"
            placeholder="Min 0"
            value={filters.minPrice}
            onChange={(e) => onFilterChange('minPrice', e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>

        {/* Max Price */}
        <div className="filter-item">
          <label className="filter-label">Max Price ($)</label>
          <input
            type="number"
            min="0"
            step="1"
            className="input-field"
            placeholder="Max 10000"
            value={filters.maxPrice}
            onChange={(e) => onFilterChange('maxPrice', e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>

        {/* Sort By */}
        <div className="filter-item">
          <label className="filter-label">Sort By</label>
          <select
            className="input-field select-field"
            value={`${filters.sortBy}_${filters.sortDescending}`}
            onChange={(e) => {
              const [sortBy, sortDesc] = e.target.value.split('_');
              onFilterChange('sortBy', sortBy);
              onFilterChange('sortDescending', sortDesc === 'true');
            }}
          >
            <option value="CreatedAt_true">Newest First</option>
            <option value="CreatedAt_false">Oldest First</option>
            <option value="Price_false">Price: Low to High</option>
            <option value="Price_true">Price: High to Low</option>
            <option value="Name_false">Name: A to Z</option>
            <option value="Name_true">Name: Z to A</option>
          </select>
        </div>

        {/* Action Buttons */}
        <div className="filter-actions">
          <button className="btn btn-primary" onClick={onSearch}>
            Search
          </button>
          <button className="btn btn-outline" onClick={onReset}>
            Reset
          </button>
        </div>
      </div>
    </div>
  );
}
