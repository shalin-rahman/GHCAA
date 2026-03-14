import { TestBed } from '@angular/core/testing';
import {
  BrowserTestingModule,
  platformBrowserTesting,
} from '@angular/platform-browser/testing';

if (!(TestBed as any).platform) {
  TestBed.initTestEnvironment(
    BrowserTestingModule,
    platformBrowserTesting(),
  );
}
