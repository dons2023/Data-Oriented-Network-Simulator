from django.contrib import admin
from django.contrib.auth.admin import UserAdmin
from django.utils.translation import gettext_lazy as _
from .models import Users


class CustomUserAdmin(UserAdmin):
    # super()
    fieldsets = (
        (None, {'fields': ('username', 'password', 'token')},),
        (_('Personal info'), {'fields': ('first_name', 'last_name', 'email')}),
        (_('Permissions'), {'fields': ('is_active', 'is_staff', 'is_superuser',
                                       'groups', 'user_permissions')}),
        (_('Important datas'), {'fields': ('last_login', 'date_joined')}),
    )

    list_display = ['id', 'username', 'last_login', 'is_active', 'is_superuser', 'is_staff', 'token']


admin.site.register(Users, CustomUserAdmin)
