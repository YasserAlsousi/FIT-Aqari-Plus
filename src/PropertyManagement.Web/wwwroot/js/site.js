// Property Management System - Main JavaScript

// Initialize tooltips and RTL
document.addEventListener('DOMContentLoaded', function() {
    // Force RTL for Arabic language
    const htmlLang = document.documentElement.lang;
    if (htmlLang === 'ar-EG' || htmlLang === 'ar' || htmlLang.startsWith('ar-')) {
        document.documentElement.setAttribute('dir', 'rtl');
        document.body.classList.add('rtl');

        // Update Bootstrap classes for RTL
        const elements = document.querySelectorAll('.me-2, .ms-2, .me-3, .ms-3');
        elements.forEach(function(el) {
            if (el.classList.contains('me-2')) {
                el.classList.remove('me-2');
                el.classList.add('ms-2');
            }
            if (el.classList.contains('ms-3')) {
                el.classList.remove('ms-3');
                el.classList.add('me-3');
            }
        });
    }

    // Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-hide alerts
    setTimeout(function() {
        var alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(function(alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

// Utility Functions
const PropertyManagement = {
    // Format currency
    formatCurrency: function(amount, currency = 'EGP') {
        return new Intl.NumberFormat('ar-EG', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 0
        }).format(amount);
    },
    
    // Format date
    formatDate: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('ar-EG');
    },
    
    // Show loading spinner
    showLoading: function(element) {
        const originalText = element.innerHTML;
        element.innerHTML = '<span class="loading-spinner me-2"></span>جاري التحميل...';
        element.disabled = true;
        return originalText;
    },
    
    // Hide loading spinner
    hideLoading: function(element, originalText) {
        element.innerHTML = originalText;
        element.disabled = false;
    },
    
    // Show toast notification
    showToast: function(message, type = 'success') {
        const toastContainer = document.getElementById('toast-container') || this.createToastContainer();
        const toast = this.createToast(message, type);
        toastContainer.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        // Remove toast after it's hidden
        toast.addEventListener('hidden.bs.toast', function() {
            toast.remove();
        });
    },
    
    // Create toast container
    createToastContainer: function() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1055';
        document.body.appendChild(container);
        return container;
    },
    
    // Create toast element
    createToast: function(message, type) {
        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.setAttribute('role', 'alert');
        
        const iconClass = type === 'success' ? 'fa-check-circle text-success' : 
                         type === 'error' ? 'fa-exclamation-circle text-danger' :
                         type === 'warning' ? 'fa-exclamation-triangle text-warning' :
                         'fa-info-circle text-info';
        
        toast.innerHTML = `
            <div class="toast-header">
                <i class="fas ${iconClass} me-2"></i>
                <strong class="me-auto">إشعار</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;
        
        return toast;
    },
    
    // Confirm dialog
    confirm: function(message, callback) {
        if (confirm(message)) {
            callback();
        }
    },
    
    // AJAX helper
    ajax: function(url, options = {}) {
        const defaultOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };
        
        const finalOptions = { ...defaultOptions, ...options };
        
        return fetch(url, finalOptions)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .catch(error => {
                console.error('AJAX Error:', error);
                this.showToast('حدث خطأ في الاتصال بالخادم', 'error');
                throw error;
            });
    }
};

// Property-specific functions
const PropertyModule = {
    // Delete property
    deleteProperty: function(id, title) {
        PropertyManagement.confirm(
            `هل أنت متأكد من حذف العقار "${title}"؟`,
            function() {
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = `/Properties/Delete/${id}`;
                
                const token = document.querySelector('input[name="__RequestVerificationToken"]');
                if (token) {
                    form.appendChild(token.cloneNode());
                }
                
                document.body.appendChild(form);
                form.submit();
            }
        );
    },
    
    // Filter properties
    filterProperties: function() {
        const form = document.getElementById('filter-form');
        if (form) {
            form.submit();
        }
    },
    
    // Load property details
    loadPropertyDetails: function(id) {
        const modal = document.getElementById('property-modal');
        const modalBody = modal.querySelector('.modal-body');
        
        modalBody.innerHTML = '<div class="text-center"><div class="loading-spinner"></div> جاري التحميل...</div>';
        
        PropertyManagement.ajax(`/Properties/Details/${id}`)
            .then(data => {
                modalBody.innerHTML = data;
            })
            .catch(() => {
                modalBody.innerHTML = '<div class="alert alert-danger">فشل في تحميل تفاصيل العقار</div>';
            });
    }
};

// Contract-specific functions
const ContractModule = {
    // Calculate contract end date
    calculateEndDate: function() {
        const startDate = document.getElementById('StartDate');
        const duration = document.getElementById('Duration');
        const endDate = document.getElementById('EndDate');
        
        if (startDate.value && duration.value) {
            const start = new Date(startDate.value);
            const end = new Date(start);
            end.setMonth(end.getMonth() + parseInt(duration.value));
            
            endDate.value = end.toISOString().split('T')[0];
        }
    },
    
    // Generate payment schedule
    generatePaymentSchedule: function(contractId) {
        PropertyManagement.ajax(`/Contracts/GeneratePayments/${contractId}`, {
            method: 'POST'
        })
        .then(() => {
            PropertyManagement.showToast('تم إنشاء جدول المدفوعات بنجاح');
            location.reload();
        });
    }
};

// Payment-specific functions
const PaymentModule = {
    // Mark payment as paid
    markAsPaid: function(id) {
        const button = event.target;
        const originalText = PropertyManagement.showLoading(button);
        
        PropertyManagement.ajax(`/Payments/MarkAsPaid/${id}`, {
            method: 'POST'
        })
        .then(() => {
            PropertyManagement.showToast('تم تسجيل الدفع بنجاح');
            location.reload();
        })
        .catch(() => {
            PropertyManagement.hideLoading(button, originalText);
        });
    },
    
    // Send payment reminder
    sendReminder: function(id) {
        PropertyManagement.ajax(`/Payments/SendReminder/${id}`, {
            method: 'POST'
        })
        .then(() => {
            PropertyManagement.showToast('تم إرسال التذكير بنجاح');
        });
    }
};

// Image upload handler
const ImageUpload = {
    // Handle file selection
    handleFileSelect: function(input) {
        const files = input.files;
        const preview = document.getElementById('image-preview');
        
        if (!preview) return;
        
        preview.innerHTML = '';
        
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            
            if (file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    img.className = 'img-thumbnail me-2 mb-2';
                    img.style.width = '100px';
                    img.style.height = '100px';
                    img.style.objectFit = 'cover';
                    preview.appendChild(img);
                };
                reader.readAsDataURL(file);
            }
        }
    },
    
    // Remove image
    removeImage: function(imageId) {
        PropertyManagement.confirm(
            'هل أنت متأكد من حذف هذه الصورة؟',
            function() {
                PropertyManagement.ajax(`/PropertyImages/Delete/${imageId}`, {
                    method: 'DELETE'
                })
                .then(() => {
                    document.querySelector(`[data-image-id="${imageId}"]`).remove();
                    PropertyManagement.showToast('تم حذف الصورة بنجاح');
                });
            }
        );
    }
};

// Make functions globally available
window.PropertyManagement = PropertyManagement;
window.PropertyModule = PropertyModule;
window.ContractModule = ContractModule;
window.PaymentModule = PaymentModule;
window.ImageUpload = ImageUpload;