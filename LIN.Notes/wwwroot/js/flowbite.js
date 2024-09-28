/**
 * Abre un Drawer
 * @param {string} id - El ID del elemento del Drawer.
 * @param {object} dotnetHelper - El objeto DotNetHelper para invocar métodos de .NET.
 * @param {...string} idCloseBtn - IDs de los botones que cierran el Drawer.
 */
function showDrawer(id, dotnetHelper, ...idCloseBtn) {
    const control = document.getElementById(id);

    const options = {
        placement: "bottom",
        backdropClasses: 'bg-zinc-900 bg-opacity-50 fixed inset-0 z-30',
        onHide: () => dotnetHelper?.invokeMethodAsync("OnHide"),
        onShow: () => dotnetHelper?.invokeMethodAsync("OnShow")
    };

    const drawer = new Drawer(control, options);
    drawer.show();

    idCloseBtn.forEach(closeBtnId => {
        const closeButton = document.getElementById(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => drawer.hide());
        }
    });
}


/**
 * Muestra un Popover
 * @param {string} id - El ID del elemento del Popover.
 * @param {string} btn - El ID del elemento que activa el Popover.
 * @param {object} dotnetHelper - El objeto DotNetHelper para invocar métodos de .NET.
 */
function showPopover(id, btn, dotnetHelper) {
    const targetEl = document.getElementById(id);
    const triggerEl = document.getElementById(btn);

    const options = {
        placement: 'bottom',
        triggerType: 'hover',
        offset: 10,
        onHide: () => dotnetHelper?.invokeMethodAsync("OnHide"),
        onShow: () => dotnetHelper?.invokeMethodAsync("OnShow")
    };

    new Popover(targetEl, triggerEl, options);
}


/**
 * Muestra un Modal
 * @param {string} id - El ID del elemento del Modal.
 * @param {object} dotnetHelper - El objeto DotNetHelper para invocar métodos de .NET.
 * @param {...string} idCloseBtn - IDs de los botones que cierran el Modal.
 */
function showModal(id, dotnetHelper, ...idCloseBtn) {
    const control = document.getElementById(id);

    const options = {
        placement: 'center',
        backdrop: 'dynamic',
        backdropClasses: 'bg-transparent fixed inset-0 z-40',
        closable: true,
        onHide: () => dotnetHelper?.invokeMethodAsync("OnHide"),
        onShow: () => dotnetHelper?.invokeMethodAsync("OnShow")
    };

    const modal = new Modal(control, options);
    modal.show();

    idCloseBtn.forEach(closeBtnId => {
        const closeButton = document.getElementById(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => modal.hide());
        }
    });
}


/**
 * Abre un Dropdown
 * @param {string} id - El ID del elemento del Dropdown.
 * @param {string} idOpen - El ID del elemento que activa el Dropdown.
 * @param {object} dotnetHelper - El objeto DotNetHelper para invocar métodos de .NET.
 * @param {...string} idCloseBtn - IDs de los botones que cierran el Dropdown.
 */
function openDropDown(id, idOpen, dotnetHelper, ...idCloseBtn) {
    const targetEl = document.getElementById(id);
    const triggerEl = document.getElementById(idOpen);

    const options = {
        placement: 'bottom',
        triggerType: 'click',
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => dotnetHelper?.invokeMethodAsync("OnHide"),
        onShow: () => dotnetHelper?.invokeMethodAsync("OnShow")
    };

    const dropdown = new Dropdown(targetEl, triggerEl, options);

    idCloseBtn.forEach(closeBtnId => {
        const closeButton = document.getElementById(closeBtnId);
        if (closeButton) {
            closeButton.addEventListener("click", () => dropdown.hide());
        }
    });

    dropdown.toggle();
}


/**
 * Abre un Dropdown específico para el usuario
 */
function showUserDropdown(dotnetHelper) {
    const targetEl = document.getElementById('user-dropdown');
    const triggerEl = document.getElementById('user-menu-button');

    const options = {
        placement: 'bottom',
        triggerType: 'click',
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => dotnetHelper?.invokeMethodAsync("OnHide"),
        onShow: () => dotnetHelper?.invokeMethodAsync("OnShow"),
        onToggle: () => dotnetHelper?.invokeMethodAsync("OnToggle")
    };

    const dropdown = new Dropdown(targetEl, triggerEl, options);
    dropdown.toggle();
}


/**
 * Abre o cierra el menú móvil
 */
function toggleMobileMenu() {
    const targetEl = document.getElementById('mobile-menu-2');
    const collapse = new Collapse(targetEl, targetEl);
    collapse.toggle();
}