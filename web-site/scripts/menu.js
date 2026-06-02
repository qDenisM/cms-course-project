document.addEventListener('DOMContentLoaded', function() {
    var menuToggle = document.getElementById('menuToggle');
    var mainMenu = document.getElementById('mainMenu');
    
    if (!menuToggle || !mainMenu) return;
    
    function isMobile() {
        return window.innerWidth <= 770;
    }
    
    function toggleMenu() {
        if (mainMenu.classList.contains('active')) {
            mainMenu.classList.remove('active');
            document.body.style.overflow = '';
        } else {
            mainMenu.classList.add('active');
            if (isMobile()) {
                document.body.style.overflow = 'hidden';
            }
        }
    }
    
    function closeMenu() {
        mainMenu.classList.remove('active');
        document.body.style.overflow = '';
    }
    
    menuToggle.addEventListener('click', function(event) {
        event.preventDefault();
        event.stopPropagation();
        toggleMenu();
    });
    
    var menuLinks = mainMenu.getElementsByTagName('a');
    for (var i = 0; i < menuLinks.length; i++) {
        menuLinks[i].addEventListener('click', function() {
            if (isMobile()) {
                closeMenu();
            }
        });
    }
    
    document.addEventListener('click', function(event) {
        if (isMobile() && mainMenu.classList.contains('active')) {
            if (event.target !== menuToggle && !mainMenu.contains(event.target)) {
                closeMenu();
            }
        }
    });
    
    window.addEventListener('resize', function() {
        if (!isMobile()) {
            closeMenu();
        }
    });
});
