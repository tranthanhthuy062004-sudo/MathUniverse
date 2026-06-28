// ========================================
// VŨ TRỤ TOÁN HỌC - MAIN JAVASCRIPT
// ========================================

(function() {
    'use strict';

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Vũ trụ Toán học - Initialized');
    });
})();

// ========================================
// SCROLL ANIMATIONS
// ========================================
})();

// ========================================
// SMOOTH SCROLL
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

// ========================================
// FADE IN ANIMATION ON SCROLL
// ========================================

const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

document.addEventListener('DOMContentLoaded', function() {
    // Observe cards and other elements
    document.querySelectorAll('.card, .hero-section').forEach(el => {
        observer.observe(el);
    });
});

// ========================================
// NOTIFICATION AUTO DISMISS
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    // Auto dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});
