document.addEventListener('DOMContentLoaded', function() {
    const contentImages = document.querySelectorAll('.content-image');
    contentImages.forEach(function(img) {
        img.addEventListener('click', function() {
            if (this.classList.contains('enlarged')) {
                this.classList.remove('enlarged');
            } else {
                contentImages.forEach(function(otherImg) {
                    otherImg.classList.remove('enlarged');
                });
                this.classList.add('enlarged');
            }
        });
        
        img.addEventListener('mouseleave', function() {
            this.classList.remove('enlarged');
        });
    });

    const textReplaces = document.querySelectorAll('.text-replace');
    textReplaces.forEach(function(element) {
        const originalText = element.textContent;
        const imageSrc = element.getAttribute('data-image');
        const imageAlt = element.getAttribute('data-alt') || originalText;
        
        if (imageSrc) {
            element.addEventListener('click', function() {
                if (this.classList.contains('replaced')) {
                    this.innerHTML = originalText;
                    this.classList.remove('replaced');
                } else {
                    const img = document.createElement('img');
                    img.src = imageSrc;
                    img.alt = imageAlt;
                    this.innerHTML = '';
                    this.appendChild(img);
                    this.classList.add('replaced');
                }
            });
            
            element.addEventListener('mouseleave', function() {
                if (this.classList.contains('replaced')) {
                    this.innerHTML = originalText;
                    this.classList.remove('replaced');
                }
            });
        }
    });
    
    const partItems = document.querySelectorAll('.parts-list li');
    const partsViewer = document.querySelector('.parts-viewer');
    
    if (partItems.length > 0 && partsViewer) {
        partItems.forEach(function(item) {
            item.addEventListener('click', function() {
                partItems.forEach(function(otherItem) {
                    otherItem.classList.remove('active');
                });
                
                this.classList.add('active');
                
                const imageSrc = this.getAttribute('data-image');
                const partName = this.textContent.trim();
                
                if (imageSrc) {
                    partsViewer.innerHTML = '';
                    
                    const img = document.createElement('img');
                    img.src = imageSrc;
                    img.alt = partName;
                    img.onerror = function() {
                        partsViewer.innerHTML = '<div class="parts-viewer-placeholder">Изображение не найдено</div>';
                    };
                    partsViewer.appendChild(img);
                } else {
                    partsViewer.innerHTML = '<div class="parts-viewer-placeholder">Изображение не указано</div>';
                }
            });
        });
    }
});
