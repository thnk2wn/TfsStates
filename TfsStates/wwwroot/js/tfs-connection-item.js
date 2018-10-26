function initNewConnection($connRoot) {
    var $form = $connRoot.find('form');
    var $connType = $connRoot.find('.connection-type');
    var $useDefaultCreds = $connRoot.find('.use-default-credentials');
    onConnectionTypeChange($connType[0]);
    onWindowsIdentityChange($useDefaultCreds[0]);
    var id = $connRoot.attr('data-id');

    $form.submit(function (e) {
        e.preventDefault();
        var data = $form.serialize();

        $.post($form[0].action, data, function (savedConnectionView) {
            $connRoot.replaceWith(savedConnectionView);

            var $newForm = $('body').find('#connectionForm_' + id);
            var $replaced = $newForm.closest('.connection-root');
            initNewConnection($replaced);
        });
    });
}

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
    var $form = $element.closest('.connection-root');
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
    $form.find(".tfs-settings-validate-success").hide();

    var $savingLabel = $form.find(".saving-label");
    $savingLabel.show();
    $savingLabel.html('<p>Saving and testing connection, please wait...</p>');

    $form.find(".save-button").prop('disabled', true);
    $form.find(".delete-button").prop('disabled', true);
}

function afterSave($form) {
    $form.find(".save-button").prop('disabled', false);
    $form.find(".delete-button").prop('disabled', false);
    $form.find(".saving-label").hide();
}