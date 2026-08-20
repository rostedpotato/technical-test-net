import React, { useState, useEffect, useRef } from 'react';

export default function FilterBar({ filters, onFilterChange, onReset }) {
  // Local input state for smooth typing without lag
  const [keywordInput, setKeywordInput] = useState(filters.keyword || '');
  const [minPriceInput, setMinPriceInput] = useState(filters.minPrice !== undefined ? filters.minPrice : '');
  const [maxPriceInput, setMaxPriceInput] = useState(filters.maxPrice !== undefined ? filters.maxPrice : '');
  const [isDebouncing, setIsDebouncing] = useState(false);

  const isInitialMount = useRef(true);

  // Sync local inputs if external filters are reset or changed
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setKeywordInput(filters.keyword || '');
    setMinPriceInput(filters.minPrice !== undefined ? filters.minPrice : '');
    setMaxPriceInput(filters.maxPrice !== undefined ? filters.maxPrice : '');
  }, [filters.keyword, filters.minPrice, filters.maxPrice]);

  // Debounce for Keyword, MinPrice, and MaxPrice (400ms delay)
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }

    setIsDebouncing(true);
    const handler = setTimeout(() => {
      onFilterChange({
        keyword: keywordInput,
        minPrice: minPriceInput,
        maxPrice: maxPriceInput
      });
      setIsDebouncing(false);
    }, 400); // 400ms debounce delay

    return () => {
      clearTimeout(handler);
    };
  }, [keywordInput, minPriceInput, maxPriceInput, onFilterChange]);

  const handleResetClick = () => {
    setKeywordInput('');
    setMinPriceInput('');
    setMaxPriceInput('');
    onReset();
  };

  return (
    <div className="filter-card">
      <div className="filter-grid">
        {/* Keyword Search */}
        <div className="filter-item filter-search">
          <div className="filter-label-row">
            <label className="filter-label">Search Product</label>
            {isDebouncing && <span className="debouncing-indicator">⏳ Searching...</span>}
          </div>
          <div className="search-input-wrapper">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              className="input-field search-input"
              placeholder="Search by name or description (auto debounced)..."
              value={keywordInput}
              onChange={(e) => setKeywordInput(e.target.value)}
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
            value={minPriceInput}
            onChange={(e) => setMinPriceInput(e.target.value)}
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
            value={maxPriceInput}
            onChange={(e) => setMaxPriceInput(e.target.value)}
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
              onFilterChange({
                sortBy: sortBy,
                sortDescending: sortDesc === 'true'
              });
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
          <button className="btn btn-outline" onClick={handleResetClick} title="Reset all filters">
            🔄 Reset
          </button>
        </div>
      </div>
    </div>
  );
}
