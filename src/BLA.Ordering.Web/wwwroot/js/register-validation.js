(function () {
    'use strict';

    var PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;

    var form = document.getElementById('register-form');
    if (!form) return;

    var emailInput = document.getElementById('Email');
    var passwordInput = document.getElementById('Password');
    var confirmInput = document.getElementById('ConfirmPassword');
    var submitBtn = document.getElementById('submit-btn');

    function showError(inputEl, errorId, message) {
        var span = document.getElementById(errorId);
        if (!span) return;
        span.textContent = message;
        inputEl.setAttribute('aria-invalid', 'true');
    }

    function clearError(inputEl, errorId) {
        var span = document.getElementById(errorId);
        if (!span) return;
        span.textContent = '';
        inputEl.setAttribute('aria-invalid', 'false');
    }

    function validateEmail() {
        var value = emailInput.value.trim();
        if (!value) {
            showError(emailInput, 'Email-error', 'Email is required.');
            return false;
        }
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
            showError(emailInput, 'Email-error', 'Please enter a valid email address.');
            return false;
        }
        clearError(emailInput, 'Email-error');
        return true;
    }

    function validatePassword() {
        var value = passwordInput.value;
        if (!value) {
            showError(passwordInput, 'Password-error', 'Password is required.');
            return false;
        }
        if (!PASSWORD_PATTERN.test(value)) {
            showError(passwordInput, 'Password-error',
                'Password must be at least 8 characters with uppercase, lowercase, a number, and a special character.');
            return false;
        }
        clearError(passwordInput, 'Password-error');
        return true;
    }

    function validateConfirm() {
        if (confirmInput.value !== passwordInput.value) {
            showError(confirmInput, 'ConfirmPassword-error', 'Passwords do not match.');
            return false;
        }
        clearError(confirmInput, 'ConfirmPassword-error');
        return true;
    }

    function updateSubmitState() {
        var allValid = emailInput.getAttribute('aria-invalid') !== 'true'
            && passwordInput.getAttribute('aria-invalid') !== 'true'
            && confirmInput.getAttribute('aria-invalid') !== 'true'
            && emailInput.value.trim() !== ''
            && passwordInput.value !== ''
            && confirmInput.value !== '';
        submitBtn.disabled = !allValid;
    }

    emailInput.addEventListener('blur', function () { validateEmail(); updateSubmitState(); });
    emailInput.addEventListener('input', function () { if (emailInput.getAttribute('aria-invalid') === 'true') validateEmail(); updateSubmitState(); });

    passwordInput.addEventListener('input', function () { validatePassword(); if (confirmInput.value) validateConfirm(); updateSubmitState(); });

    confirmInput.addEventListener('blur', function () { validateConfirm(); updateSubmitState(); });
    confirmInput.addEventListener('input', function () { if (confirmInput.getAttribute('aria-invalid') === 'true') validateConfirm(); updateSubmitState(); });

    form.addEventListener('submit', function (e) {
        var valid = validateEmail() & validatePassword() & validateConfirm();
        if (!valid) {
            e.preventDefault();
            // Move focus to the first invalid field
            var firstInvalid = form.querySelector('[aria-invalid="true"]');
            if (firstInvalid) firstInvalid.focus();
        }
    });
})();
