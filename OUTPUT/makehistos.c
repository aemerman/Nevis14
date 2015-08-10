const char* filepath = "C:\\Users/Max Beck/My Documents/Physics/Chip Testing/Nevis14/OUTPUT/";

void makehistos()
{
	//static bool loaded = false;
	//if(loaded) gROOT->ProcessLine(".U AtlasStyle.C");
	gROOT->SetStyle("ATLAS");
	SetAtlasStyle();
	gStyle->SetOptStat(1);
	
	TH1I *rangehisto;
	TH1D *enobhisto;
	TH1D *sfdrhisto;
	TH1D *sinadhisto;
	TH1D *snrhisto;

	rangehisto = new TH1I("DynamicRange", "Nevis14 Dynamic Range Distribution", 56, 3000, 4000);
	enobhisto = new TH1D("ENOB", "Nevis14 ENOB Distribution", 50, 9.0, 10.5);
	sfdrhisto = new TH1D("SFDR", "Nevis14 SFDR Distribution", 40, -77.0, -65.0);
	sinadhisto = new TH1D("SINAD", "Nevis14 SINAD Distribution", 40, -65.0, -59.0);
	snrhisto = new TH1D("SNR", "Nevis14 SNR Distribution", 40, -65.0, -59.0);

	Int_t range = 0;
	Double_t enob, sfdr, sinad, snr;
	
	ifstream qadatafile;
	qadatafile.open(Form("%sQAparams_rootformat.txt", filepath));
	
	int k = 0;
	int j = 0;
	while (!qadatafile.eof())
	{
		qadatafile >> range >> enob >> sfdr >> sinad >> snr;
		if (range > 3000 && range < 4096)
			rangehisto->Fill(range);
		else
			k++;
		if (enob > 9.3)
			enobhisto->Fill(enob);
		else
			j++;
		sfdrhisto->Fill(sfdr);
		sinadhisto->Fill(sinad);
		snrhisto->Fill(snr);
	}
	cout << k << ", " << j;
	showHistogram(rangehisto);
	showHistogram(enobhisto);
	showHistogram(sinadhisto);
	showHistogram(sfdrhisto);
	showHistogram(snrhisto);
}

void showHistogram(TH1 *h1)
{
	char canvasname [150];
	sprintf(canvasname, Form("Nevis14 %s Distribution", h1->GetName()));
	TCanvas *hcanvas = new TCanvas(canvasname, canvasname, 800, 400);
	hcanvas->SetFillColor(0);
	h1->SetFillColor(4);
	h1->GetYaxis()->SetTitle("S");
	h1->GetXaxis()->SetTitle(h1->GetName());
	h1->Draw();
	hcanvas->Update();
	char histobuffer [150];
	sprintf(histobuffer, Form("%sanalysis/%s.pdf", filepath, h1->GetName()));
	hcanvas->Print(histobuffer, "pdf");
}
	