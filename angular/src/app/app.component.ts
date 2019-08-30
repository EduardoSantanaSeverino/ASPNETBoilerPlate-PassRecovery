import { DOCUMENT } from '@angular/common';
import { Component, Injector, Inject, Renderer2, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { MatIconRegistry, MatDialog } from '@angular/material';
import { SidenavService } from './layout/sidenav/sidenav.service';
import { ThemeService } from '../@fury/services/theme.service';
import { InstaAccountServiceProxy, PagedResultDtoOfInstaAccountDto } from '@shared/service-proxies/service-proxies';
import { AppSessionService } from '@shared/session/app-session.service';
import { AppConsts } from '@shared/AppConsts';
import { Router, ActivatedRoute } from '@angular/router';
import { Platform } from '@angular/cdk/platform';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'fury-root',
  templateUrl: './app.component.html'
})
export class AppComponent extends AppComponentBase implements OnInit {

  ngOnInit(): void {

    // Check for redirect to other views.
    let navigateTo = this.checkForPermanentLinks();
    if (navigateTo.length > 1) {
      this._router.navigate([navigateTo]);
    } else {
      if (this._sessionService.user) {
        let showAddAccountButton: boolean = abp.setting.getBoolean("ShowAddAccountButton");
        let showCampaignsButton: boolean = abp.setting.getBoolean("ShowCampaignsButton");
        this._instaaccountsService
          .getAll("", true, 0, 5)
          .subscribe((result: PagedResultDtoOfInstaAccountDto) => {

            let index = 6;

            for (let i of result.items) {

              this.sidenavService.addItem({
                name: i.userName,
                routeOrFunction: '/instaaccounts/info/' + i.id,
                icon: 'face',
                profilePicture: i.profilePicture,
                position: index
              });

              index++;
            }

            if (showAddAccountButton) {
              this.sidenavService.addItem({
                name: 'Connect Account',
                routeOrFunction: '/instaaccounts/create',
                icon: 'add',
                position: index
              });
            }

            if (showCampaignsButton) {

              this.sidenavService.addItem({
                name: 'APPS',
                position: 20,
                type: 'subheading'
              });

              this.sidenavService.addItem({
                name: 'Campaigns',
                routeOrFunction: '/campaigns',
                icon: 'assignment',
                position: 21
              });

            }

          });

      } else {
        this._router.navigate([AppConsts.loginUrl]);
      }
    }

  }

  constructor(
    private injector: Injector,
    private sidenavService: SidenavService,
    private iconRegistry: MatIconRegistry,
    private renderer: Renderer2,
    private themeService: ThemeService,
    private _instaaccountsService: InstaAccountServiceProxy,
    private _sessionService: AppSessionService,
    private _router: Router,
    private _dialog: MatDialog,
    private platform: Platform,
    private route: ActivatedRoute,
    @Inject(DOCUMENT) private document: Document) {
    super(injector);
    this.route.queryParamMap.pipe(
      filter(queryParamMap => queryParamMap.has('style'))
    ).subscribe(queryParamMap => this.themeService.setStyle(queryParamMap.get('style')));
    this.iconRegistry.setDefaultFontSetClass('material-icons');
    this.themeService.theme$.subscribe(theme => {
      if (theme[0]) {
        this.renderer.removeClass(this.document.body, theme[0]);
      }

      this.renderer.addClass(this.document.body, theme[1]);
    });

    if (this.platform.BLINK) {
      this.renderer.addClass(this.document.body, 'is-blink');
    }

    this.sidenavService.addItems([
      {
        name: 'DASHBOARD',
        position: 0,
        type: 'subheading',
        customClass: 'first-subheading'
      },
      {
        name: 'Dashboard',
        routeOrFunction: '/instaaccounts',
        icon: 'dashboard',
        position: 1,
        pathMatchExact: true
      },
      {
        name: 'ACCOUNTS',
        position: 5,
        type: 'subheading'
      }
    ]);
  }

  private checkForPermanentLinks(): string {

    const parameters = new URLSearchParams(window.location.search);
    let urlString: string = '';
    parameters.forEach((value, key) => {
      urlString += '/' + value;
    });

    if (urlString.length > 1) {
      return urlString;
    }

    return '';

  }

  private getQueryParameter(key: string): string {
    const parameters = new URLSearchParams(window.location.search);
    return parameters.get(key);
  }
}
