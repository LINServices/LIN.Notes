// Abre un Drawer
function ShowDrawer(id, ...idCloseBtn) {

    // Control
    const control = document.getElementById(id);

    const options = {
        placement: "bottom",
        backdropClasses : 'bg-zinc-900 bg-opacity-50 fixed inset-0 z-30'
    };


    const drawer = new Drawer(control, options);

    drawer.show();

    for (let i = 0; i < idCloseBtn.length; i++) {

        try {

            let closeButton = document.getElementById(idCloseBtn[i]);

            closeButton.addEventListener("click", () => {

                drawer.hide();

            });
        } catch {

        }

    }

}

// Abre
function ForceClick(id) {

    const control = document.getElementById(id);
    control.click();
}


function ShowPop(id, btn) {
    // set the popover content element
    const $targetEl = document.getElementById(id);

    // set the element that trigger the popover using hover or click
    const $triggerEl = document.getElementById(btn);

    // options with default values
    const options = {
        placement: 'bottom',
        triggerType: 'hover',
        offset: 10,
        onHide: () => {
            console.log('popover is shown');
        },
        onShow: () => {
            console.log('popover is hidden');
        },
        onToggle: () => {
            console.log('popover is toggled');
        }
    };

    const popover = new Popover($targetEl, $triggerEl, options);

    popover.show();
}


// Abre un

// Abre
function ShowModal(id, ...idCloseBtn) {

    // Control
    const control = document.getElementById(id);


    const drawer = new Modal(control);

    // show the drawer
    drawer.show();


    for (let i = 0; i < idCloseBtn.length; i++) {

        try {
            let closeButton = document.getElementById(idCloseBtn[i]);


            closeButton.addEventListener("click", () => {

                drawer.hide();

            });
        } catch {

        }

    }


}


function OpenDropDown(id, idOpen, ...idCloseBtn) {


    // set the target element that will be collapsed or expanded (eg. navbar menu)
    const $targetEl = document.getElementById(id);
    const $targetEl2 = document.getElementById(idOpen);

    // options with default values
    const options = {
        placement: 'bottom',
        triggerType: 'click',
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => {
            console.log('dropdown has been hidden');
        },
        onShow: () => {
            console.log('dropdown has been shown');
        },
        onToggle: () => {
            console.log('dropdown has been toggled');
        }
    };

    let collapse = new Dropdown($targetEl, $targetEl2, options);


    for (let i = 0; i < idCloseBtn.length; i++) {

        try {
            let closeButton = document.getElementById(idCloseBtn[i]);


            closeButton.addEventListener("click", () => {

                collapse.hide();

            });
        } catch {

        }

    }


    // show the target element
    collapse.toggle();


}


function E() {


    // set the target element that will be collapsed or expanded (eg. navbar menu)
    const $targetEl = document.getElementById('user-dropdown');

    const $targetEl2 = document.getElementById('user-menu-button');


    // options with default values
    const options = {
        placement: 'bottom',
        triggerType: 'click',
        offsetSkidding: 0,
        offsetDistance: 10,
        delay: 100,
        onHide: () => {
            console.log('dropdown has been hidden');
        },
        onShow: () => {
            console.log('dropdown has been shown');
        },
        onToggle: () => {
            console.log('dropdown has been toggled');
        }
    };

    let collapse = new Dropdown($targetEl, $targetEl2, options);

    // show the target element
    collapse.toggle();


}


function I() {


    // set the target element that will be collapsed or expanded (eg. navbar menu)
    const $targetEl = document.getElementById('mobile-menu-2');


    const collapse1 = new Collapse($targetEl, $targetEl);

    // show the target element
    collapse1.toggle();

}


function ShopPop(target, trigger) {


    console.log('Target: ' + target);
    console.log('Trigger: ' + trigger);

    // set the popover content element
    const $targetEl = document.getElementById(target);

    // set the element that trigger the popover using hover or click
    const $triggerEl = document.getElementById(trigger);

    // options with default values
    const options = {
        placement: 'bottom',
        triggerType: 'hover',
        offset: 10,
        onHide: () => {
            console.log('popover is shown');
        },
        onShow: () => {
            console.log('popover is hidden');
        },
        onToggle: () => {
            console.log('popover is toggled');
        }
    };


    const popover = new Popover($targetEl, $triggerEl, options);


}