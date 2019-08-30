import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { fadeInUpAnimation } from '../../../../@fury/animations/fade-in-up.animation';
import { UserServiceProxy, ForgotPasswordDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'fury-forgot-password',
    templateUrl: './forgot-password.component.html',
    styleUrls: ['./forgot-password.component.scss'],
    animations: [fadeInUpAnimation]
})
export class ForgotPasswordComponent implements OnInit {

    isSending: boolean = false;
    emailSended: boolean = false;

    form = this.fb.group({
        email: [null, Validators.required]
    });

    constructor(
        private router: Router,
        private fb: FormBuilder,
        private _usersService: UserServiceProxy,
    ) { }

    ngOnInit(): void {

    }

    send(): void {

        this.isSending = true;

        let createForgotPassword = new ForgotPasswordDto();

        createForgotPassword.email = this.form.value.email;

        this._usersService
            .forgotPassword(createForgotPassword)
            .pipe(
                finalize(() => {
                    this.isSending = false;
                })
            )
            .subscribe((result) => {
                if (result) {
                    abp.message.info("Password Reset Email Was Send Successfully!", "Email Successfully");
                    this.emailSended = true;
                    //this.router.navigate(['/']);
                } else {
                    abp.message.error("Please Contact Administrator", "Error");
                    //this.router.navigate(['/']);
                }
            });
        
        
    }
}
