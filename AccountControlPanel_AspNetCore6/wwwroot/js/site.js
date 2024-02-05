document.addEventListener('DOMContentLoaded', function () {
        var forgotPasswordLink = document.getElementById('forgotPasswordLink');
    var usernameInput = document.getElementById('usernameInput');

    forgotPasswordLink.addEventListener('click', function(event) {
            var username = usernameInput.value.trim();
    if(username === '') {
        event.preventDefault(); // Linkin takip edilmesini önle
    alert('Lütfen kullanıcı adınızı giriniz.');
            } else {
        this.href = '/Home/ForgotPassword?username=' + encodeURIComponent(username);
            }
        });
    });