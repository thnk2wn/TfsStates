function onWindowsIdentityChange(checked) {
    var username = $("#Username");
    var password = $("#Password");

    username.prop('disabled', checked);
    password.prop('disabled', checked);

    if (checked) {
        username.val('');
        password.val('');
    }
}

function getForm(element) {
    var $element = $(element);
    var $form = $element.closest('form');
    return $form;
}

function onConnectionTypeChange(element) {
    const typeNtlm = 'TFS Server - NTLM';
    const typeDevOpsAD = 'Azure DevOps - Active Directory';

    var $form = getForm(element);
    var defaultCredsRow = $form.find('.default-creds-row');
    var userCredsRow = $form.find('.user-creds-row');
    var tokenRow = $form.find('.token-row');
    var type = element.value;

    if (type === typeNtlm || type === typeDevOpsAD) {
        defaultCredsRow.show();
        userCredsRow.show();
        tokenRow.hide();
    }
    else {
        defaultCredsRow.hide();
        userCredsRow.hide();
        tokenRow.show();
    }
}