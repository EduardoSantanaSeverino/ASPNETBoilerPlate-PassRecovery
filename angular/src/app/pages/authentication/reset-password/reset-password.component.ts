import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { fadeInUpAnimation } from '../../../../@fury/animations/fade-in-up.animation';
import { UserServiceProxy, ForgotPasswordDto, ResetPasswordEmailDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'fury-reset-password',
    templateUrl: './reset-password.component.html',
    styleUrls: ['./reset-password.component.scss'],
    animations: [fadeInUpAnimation]
})
export class ResetPasswordComponent implements OnInit {

    inputType = 'password';
    visible = false;
    resetPassword: ResetPasswordEmailDto = new ResetPasswordEmailDto();
    isSending: boolean = false;
    passwordChanged: boolean = false;
    form: FormGroup;

    constructor(
        private router: Router,
        private fb: FormBuilder,
        private _usersService: UserServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private cd: ChangeDetectorRef,
    ) {
    }

    ngOnInit(): void {
        debugger;
        this._activatedRoute.params.subscribe((params: Params) => {
            this.resetPassword.userId = params['userId'];
            this.resetPassword.token = params['token'];
            this.form = this.fb.group({
                email: [params['userName'], Validators.required],
                password: [null, Validators.required],
                passwordConfirm: [null, Validators.required],
            });
        });
    }

    toggleVisibility(): void {
        if (this.visible) {
            this.inputType = 'password';
            this.visible = false;
            this.cd.markForCheck();
        } else {
            this.inputType = 'text';
            this.visible = true;
            this.cd.markForCheck();
        }
    }

    send(): void {

        this.isSending = true;
        this.resetPassword.newPassword = this.form.value.password;
        this.resetPassword.confirmPassword = this.form.value.passwordConfirm;

        const createResetPassword = new ResetPasswordEmailDto();
        createResetPassword.init(this.resetPassword);

        this._usersService
        .resetPasswordEmail(createResetPassword)
        .pipe(
            finalize(() => {
                this.isSending = false;
            })
        )
        .subscribe((result) => {
            if (result) {
                abp.message.info("Password Changed Successfully!", "Password Changed");
                this.passwordChanged = true;
                //this.router.navigate(['/']);
            } else {
                abp.message.error("Please Contact Administrator", "Error");
                //this.router.navigate(['/']);
            }
        });
        
    }
}
