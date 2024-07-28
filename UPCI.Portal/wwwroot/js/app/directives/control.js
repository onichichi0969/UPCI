
app.directive('user-autocomplete', {
    mounted(el, binding) {
        $(el).autocomplete({
            minLength: 2,
            source: (request, response) => {
                const filteredUsers = binding.value.filter(user => {
                    const fullname = `${user.firstName} ${user.lastName}`.toLowerCase();
                    return fullname.includes(request.term.toLowerCase());
                });

                const options = filteredUsers.map(user => ({
                    label: `${user.firstName} ${user.lastName}`,
                    value: user.id
                }));

                response(options);
            },
            select: (event, ui) => {
                app.config.globalProperties.selectedUser = ui.item.value;
            }
        });
    }
});
 