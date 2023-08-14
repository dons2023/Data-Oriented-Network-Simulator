from django.contrib.auth.models import AbstractUser
from django.db import models


class Users(AbstractUser):
    token = models.CharField(max_length=128, blank=True)

    def __str__(self):
        return self.username