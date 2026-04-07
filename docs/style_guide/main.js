/**
 * HnVue Style Guide - Main JavaScript
 * Provides interactive functionality for the style guide documentation
 */

// Copy code functionality
function copyCode(button) {
    const codeBlock = button.parentElement.nextElementSibling;
    const code = codeBlock.querySelector('code').textContent;
    navigator.clipboard.writeText(code).then(() => {
        button.textContent = 'Copied!';
        setTimeout(() => button.textContent = 'Copy', 2000);
    });
}

// Active navigation highlighting
document.addEventListener('DOMContentLoaded', function() {
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    const navLinks = document.querySelectorAll('.nav-menu a');

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPage) {
            link.classList.add('active');
        }
    });
});

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Search functionality (optional enhancement)
function initializeSearch() {
    const searchInput = document.getElementById('search-input');
    if (!searchInput) return;

    let searchTimeout;
    searchInput.addEventListener('input', function(e) {
        clearTimeout(searchTimeout);
        const query = e.target.value.toLowerCase();

        searchTimeout = setTimeout(() => {
            const sections = document.querySelectorAll('.section');
            sections.forEach(section => {
                const text = section.textContent.toLowerCase();
                if (query.length < 2 || text.includes(query)) {
                    section.style.display = '';
                } else {
                    section.style.display = 'none';
                }
            });
        }, 300);
    });
}

// Print-friendly enhancements
function enhancePrintOutput() {
    window.addEventListener('beforeprint', function() {
        // Expand collapsed sections before printing
        document.querySelectorAll('details[open="false"]').forEach(detail => {
            detail.setAttribute('data-was-closed', 'true');
            detail.open = true;
        });
    });

    window.addEventListener('afterprint', function() {
        // Restore collapsed sections after printing
        document.querySelectorAll('details[data-was-closed="true"]').forEach(detail => {
            detail.open = false;
            detail.removeAttribute('data-was-closed');
        });
    });
}

// Keyboard navigation shortcuts
function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Alt + Left/Right for page navigation
        if (e.altKey && !e.shiftKey && !e.ctrlKey) {
            const currentPage = window.location.pathname.split('/').pop() || 'index.html';
            const pages = ['index.html', 'visual-identity.html', 'components.html', 'patterns.html', 'accessibility.html', 'api.html'];
            const currentIndex = pages.indexOf(currentPage);

            if (e.key === 'ArrowLeft' && currentIndex > 0) {
                window.location.href = pages[currentIndex - 1];
            } else if (e.key === 'ArrowRight' && currentIndex < pages.length - 1) {
                window.location.href = pages[currentIndex + 1];
            }
        }

        // Escape to clear search
        if (e.key === 'Escape') {
            const searchInput = document.getElementById('search-input');
            if (searchInput && document.activeElement === searchInput) {
                searchInput.value = '';
                searchInput.blur();
                // Reset visibility
                document.querySelectorAll('.section').forEach(section => {
                    section.style.display = '';
                });
            }
        }
    });
}

// Responsive sidebar toggle for mobile
function initializeMobileNavigation() {
    const sidebar = document.querySelector('.sidebar');
    const content = document.querySelector('.content');
    let isSidebarOpen = false;

    // Only add toggle button on mobile
    if (window.innerWidth <= 768) {
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'sidebar-toggle';
        toggleBtn.innerHTML = '☰';
        toggleBtn.setAttribute('aria-label', 'Toggle navigation');
        toggleBtn.style.cssText = `
            position: fixed;
            top: 16px;
            left: 16px;
            z-index: 1000;
            background: var(--primary-main);
            color: white;
            border: none;
            border-radius: 6px;
            width: 44px;
            height: 44px;
            font-size: 20px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
        `;

        document.body.appendChild(toggleBtn);

        toggleBtn.addEventListener('click', function() {
            isSidebarOpen = !isSidebarOpen;
            sidebar.style.transform = isSidebarOpen ? 'translateX(0)' : 'translateX(-100%)';
            sidebar.style.transition = 'transform 0.3s ease';
            content.style.marginLeft = isSidebarOpen ? '280px' : '0';
        });
    }
}

// Table of contents generation
function generateTableOfContents() {
    const tocContainer = document.getElementById('table-of-contents');
    if (!tocContainer) return;

    const headings = document.querySelectorAll('main h2, main h3');
    const tocList = document.createElement('ul');
    tocList.className = 'toc-list';

    headings.forEach(heading => {
        const li = document.createElement('li');
        const link = document.createElement('a');
        link.textContent = heading.textContent;
        link.href = `#${heading.id || heading.textContent.toLowerCase().replace(/\s+/g, '-')}`;
        link.className = heading.tagName.toLowerCase() === 'h3' ? 'toc-sub-item' : '';
        li.appendChild(link);
        tocList.appendChild(li);

        // Add ID if missing
        if (!heading.id) {
            heading.id = heading.textContent.toLowerCase().replace(/\s+/g, '-');
        }
    });

    tocContainer.appendChild(tocList);
}

// Intersection Observer for scroll spy
function initializeScrollSpy() {
    const headings = document.querySelectorAll('main h2[id], main h3[id]');
    const navLinks = document.querySelectorAll('.toc-list a');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const id = entry.target.id;
                navLinks.forEach(link => {
                    link.classList.toggle('active', link.getAttribute('href') === `#${id}`);
                });
            }
        });
    }, {
        rootMargin: '-100px 0px -66%',
        threshold: 0
    });

    headings.forEach(heading => observer.observe(heading));
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
    enhancePrintOutput();
    initializeKeyboardShortcuts();
    initializeMobileNavigation();
    generateTableOfContents();
    initializeScrollSpy();

    // Add loading complete class
    document.body.classList.add('loaded');
});

// Export functions for external use
window.HnVueStyleGuide = {
    copyCode,
    initializeSearch,
    enhancePrintOutput,
    initializeKeyboardShortcuts,
    initializeMobileNavigation,
    generateTableOfContents,
    initializeScrollSpy
};
