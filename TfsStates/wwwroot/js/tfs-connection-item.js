function onWindowsIdentityChange(element) {
    var checked = element.checked;
    var $root = getRootContainer(element);
    var $username = $root.find('.username');
    var $password = $root.find('.password');

    $username.prop('disabled', checked);
    $password.prop('disabled', checked);

    if (checked) {
        $username.val('');
        $password.val('');
    }
}

function getRootContainer(element) {
    var $element = $(element);
    var $form = $element.closest('form');
    return $form;
}

function onConnectionTypeChange(element) {
    const typeNtlm = 'TFS Server - NTLM';

    var $root = getRootContainer(element);
    var $defaultCredsRow = $root.find('.default-creds-row');
    var $userCredsRow = $root.find('.user-creds-row');
    var $tokenRow = $root.find('.token-row');
    var type = element.value;
    var $urlExample = $root.find('.url-example');
    var $url = $root.find('.url');

    if (type === typeNtlm) {
        $defaultCredsRow.show();
        $userCredsRow.show();
        $tokenRow.hide();
        $urlExample.html('i.e. http://server:8080/tfs/DefaultCollection');
        $url.attr('placeholder', 'http://server:8080/tfs/DefaultCollection');
    }
    else {
        $defaultCredsRow.hide();
        $userCredsRow.hide();
        $tokenRow.show();
        $urlExample.html('i.e. <em>https://dev.azure.com/org-name</em> or <em>https://domain.visualstudio.com</em>');
        $url.attr('placeholder', 'https://dev.azure.com/org-name');
    }    
}

function onSavingConnection(form) {
    var $form = $(form);
    $form.find(".save-button").prop('disabled', true);
    $form.find(".delete-button").prop('disabled', true);
    $form.find(".saving-label").text('Saving and testing connection, please wait...');
    $form.find(".tfs-settings-validate-success").hide();
}

function afterSave($form) {
    $form.find(".save-button").prop('disabled', false);
    $form.find(".delete-button").prop('disabled', false);
    $form.find(".saving-label").hide();
}