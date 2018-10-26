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
    const typeDevOpsAD = 'Azure DevOps - Active Directory';

    var $root = getRootContainer(element);
    var $defaultCredsRow = $root.find('.default-creds-row');
    var $userCredsRow = $root.find('.user-creds-row');
    var $tokenRow = $root.find('.token-row');
    var type = element.value;

    if (type === typeNtlm || type === typeDevOpsAD) {
        $defaultCredsRow.show();
        $userCredsRow.show();
        $tokenRow.hide();
    }
    else {
        $defaultCredsRow.hide();
        $userCredsRow.hide();
        $tokenRow.show();
    }
}

function onSavingConnection(form) {
    var $form = $(form);
    $form.find(".save-button").prop('disabled', true);
    $form.find(".saving-label").text('Saving and testing connection, please wait...');
    $form.find(".tfs-settings-validate-success").hide();
}